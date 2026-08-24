using System.Security.Claims;
using CarWash.Api.Data;
using CarWash.Api.DTOs;
using CarWash.Api.Models;
using CarWash.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace CarWash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICustomerRegistrationMirror _registrationMirror;
    private readonly IBookingMirror _bookingMirror;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        ICustomerRegistrationMirror registrationMirror,
        IBookingMirror bookingMirror,
        ILogger<BookingsController> logger)
    {
        _db = db;
        _userManager = userManager;
        _registrationMirror = registrationMirror;
        _bookingMirror = bookingMirror;
        _logger = logger;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterCustomer(CustomerRegistrationDto dto)
    {
        await _registrationMirror.AddAsync(
            dto.Name.Trim(),
            dto.Phone.Trim(),
            dto.Address.Trim(),
            dto.Email.Trim().ToLowerInvariant(),
            HttpContext.RequestAborted);

        return NoContent();
    }

    // Customer: create a booking
    [HttpPost]
    public async Task<ActionResult<CreateBookingResultDto>> Create(CreateBookingDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
            return BadRequest("Phone number is required.");

        var service = await _db.Services.FindAsync(dto.ServiceId);
        if (service is null || !service.IsActive) return BadRequest("Service not found.");

        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(candidate =>
            candidate.Id == dto.VehicleId && candidate.UserId == CurrentUserId);
        if (vehicle is null) return BadRequest("Select a valid vehicle.");

        var customer = await _userManager.FindByIdAsync(CurrentUserId);
        var booking = new Booking
        {
            UserId = CurrentUserId,
            VehicleId = vehicle.Id,
            ServiceId = dto.ServiceId,
            ScheduledAt = dto.ScheduledAt,
            Notes = dto.Notes,
            Address = dto.Address.Trim(),
            City = dto.City.Trim(),
            Pincode = dto.Pincode.Trim(),
            PhoneNumber = dto.PhoneNumber.Trim(),
            Status = BookingStatus.Pending
        };
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        try
        {
            await _bookingMirror.AddAsync(
                booking.Id,
                customer?.FullName ?? string.Empty,
                vehicle.Make,
                service.Name,
                service.PriceLabel,
                dto.Address.Trim(),
                dto.City.Trim(),
                dto.Pincode.Trim(),
                dto.ScheduledAt,
                dto.PhoneNumber.Trim(),
                HttpContext.RequestAborted);
        }
        catch (MySqlException exception)
        {
            _logger.LogError(exception, "Could not save booking to MySQL for user {UserId}", CurrentUserId);
            _db.Bookings.Remove(booking);
            await _db.SaveChangesAsync();
            return Problem(
                "Could not save booking data to MySQL. Check the washin_ton.customerser_booked table and connection.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return Ok(new CreateBookingResultDto(
            booking.Id,
            customer?.FullName ?? string.Empty,
            dto.PhoneNumber.Trim(),
            service.Name));
    }

    // Customer: view own bookings
    [HttpGet("my")]
    public async Task<ActionResult<List<BookingDto>>> GetMine()
    {
        var ids = await _db.Bookings.Where(b => b.UserId == CurrentUserId).Select(b => b.Id).ToListAsync();
        var result = new List<BookingDto>();
        foreach (var id in ids) result.Add(await ToDto(id));
        return Ok(result);
    }

    // Admin: view all bookings
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<BookingDto>>> GetAll()
    {
        try
        {
            var records = await _bookingMirror.GetAllAsync(HttpContext.RequestAborted);
            return Ok(records.Select(record => new BookingDto(
                record.Id,
                record.CustomerName,
                new VehicleDto(0, record.VehicleName, string.Empty, string.Empty, "Car"),
                new ServiceDto(0, record.ServiceName, string.Empty, record.ServicePrice),
                record.ScheduledAt,
                record.Status,
                null,
                record.Address,
                record.City,
                record.Pincode,
                record.PhoneNumber,
                record.ExpireDate)).ToList());
        }
        catch (MySqlException exception)
        {
            _logger.LogError(exception, "Could not load admin bookings from MySQL");
            return Problem(
                "Could not read booking data from washin_ton.customerser_booked.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    [HttpGet("summary")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AdminBookingSummaryDto>> GetSummary()
    {
        IReadOnlyList<MySqlBookingRecord> bookings;
        try
        {
            bookings = await _bookingMirror.GetAllAsync(HttpContext.RequestAborted);
        }
        catch (MySqlException exception)
        {
            _logger.LogError(exception, "Could not load booking summary from MySQL");
            return Problem(
                "Could not read booking data from washin_ton.customerser_booked.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var totalBookings = bookings.Count;
        var pendingBookings = bookings.Count(booking => booking.Status == BookingStatus.Pending.ToString());
        var activeStatuses = new[]
        {
            BookingStatus.Confirmed.ToString(),
            BookingStatus.InProgress.ToString(),
            BookingStatus.Completed.ToString()
        };
        var confirmedBookings = bookings.Count(booking => activeStatuses.Contains(booking.Status));

        return Ok(new AdminBookingSummaryDto(
            totalBookings,
            pendingBookings,
            confirmedBookings));
    }

    // Admin: update booking status
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateBookingStatusDto dto)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking is null) return NotFound();

        if (!Enum.TryParse<BookingStatus>(dto.Status, true, out var status))
            return BadRequest("Invalid status.");

        var previousStatus = booking.Status;
        booking.Status = status;
        await _db.SaveChangesAsync();

        try
        {
            await _bookingMirror.UpdateStatusAsync(id, status.ToString(), HttpContext.RequestAborted);
        }
        catch (MySqlException exception)
        {
            _logger.LogError(exception, "Could not update MySQL booking {BookingId} status", id);
            booking.Status = previousStatus;
            await _db.SaveChangesAsync();
            return Problem(
                "Could not update the booking status in washin_ton.customerser_booked.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            if (!await _bookingMirror.DeleteAsync(id, HttpContext.RequestAborted))
                return NotFound();
        }
        catch (MySqlException exception)
        {
            _logger.LogError(exception, "Could not delete MySQL booking {BookingId}", id);
            return Problem(
                "Could not delete the booking from washin_ton.customerser_booked.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var booking = await _db.Bookings.FindAsync(id);
        if (booking is not null)
        {
            _db.Bookings.Remove(booking);
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    private async Task<BookingDto> ToDto(int id)
    {
        var b = await _db.Bookings
            .Include(x => x.User)
            .Include(x => x.Vehicle)
            .Include(x => x.Service)
            .FirstAsync(x => x.Id == id);

        return new BookingDto(
            b.Id,
            b.User!.FullName,
            new VehicleDto(b.Vehicle!.Id, b.Vehicle.Make, b.Vehicle.Model, b.Vehicle.LicensePlate, b.Vehicle.Type),
            new ServiceDto(b.Service!.Id, b.Service.Name, b.Service.Description, b.Service.PriceLabel),
            b.ScheduledAt,
            b.Status.ToString(),
            b.Notes,
            b.Address,
            b.City,
            b.Pincode,
            b.PhoneNumber,
            b.ScheduledAt.AddDays(30)
        );
    }
}
