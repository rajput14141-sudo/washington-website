using System.Security.Claims;
using CarWash.Api.Data;
using CarWash.Api.DTOs;
using CarWash.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarWash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingsController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterCustomer(CustomerRegistrationDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _db.CustomerRegistrations.AnyAsync(customer => customer.Email == email))
            return BadRequest("This email is already registered.");

        _db.CustomerRegistrations.Add(new CustomerRegistration
        {
            Name = dto.Name.Trim(),
            Phone = dto.Phone.Trim(),
            Address = dto.Address.Trim(),
            Email = email
        });
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // Customer: create a booking
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<PublicBookingResultDto>> Create(CreateBookingDto dto)
    {
        var service = await _db.Services.FindAsync(dto.ServiceId);
        if (service is null || !service.IsActive) return BadRequest("Service not found.");

        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(email);
        if (user is not null && await _userManager.IsInRoleAsync(user, "Admin"))
            return BadRequest("Use a different email for customer registration.");

        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email, FullName = dto.CustomerName.Trim() };
            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return BadRequest(createResult.Errors.Select(error => error.Description));
            await _userManager.AddToRoleAsync(user, "Customer");
        }
        else
        {
            user.FullName = dto.CustomerName.Trim();
            await _userManager.UpdateAsync(user);
        }

        var vehicle = new Vehicle
        {
            UserId = user.Id,
            Make = dto.VehicleMake.Trim(),
            Model = dto.VehicleModel.Trim(),
            LicensePlate = dto.LicensePlate.Trim(),
            Type = dto.VehicleType.Trim()
        };
        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();

        var booking = new Booking
        {
            UserId = user.Id,
            VehicleId = vehicle.Id,
            ServiceId = dto.ServiceId,
            ScheduledAt = dto.ScheduledAt,
            Notes = dto.Notes,
            Address = dto.Address.Trim(),
            City = dto.City.Trim(),
            Pincode = dto.Pincode.Trim(),
            Status = BookingStatus.Pending
        };
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        if (!await _db.CustomerRegistrations.AnyAsync(customer => customer.Email == email))
        {
            _db.CustomerRegistrations.Add(new CustomerRegistration
            {
                Name = user.FullName,
                Phone = dto.Phone.Trim(),
                Address = $"{booking.Address}, {booking.City} - {booking.Pincode}",
                Email = email
            });
            await _db.SaveChangesAsync();
        }

        return Ok(new PublicBookingResultDto(await ToDto(booking.Id), user.Id));
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
        var ids = await _db.Bookings.Select(b => b.Id).ToListAsync();
        var result = new List<BookingDto>();
        foreach (var id in ids) result.Add(await ToDto(id));
        return Ok(result);
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

        booking.Status = status;
        await _db.SaveChangesAsync();
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
            new ServiceDto(b.Service!.Id, b.Service.Name, b.Service.Description, b.Service.Price),
            b.ScheduledAt,
            b.Status.ToString(),
            b.Notes,
            b.Address,
            b.City,
            b.Pincode
        );
    }
}
