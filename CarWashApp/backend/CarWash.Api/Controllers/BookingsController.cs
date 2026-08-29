using System.Security.Claims;
using CarWash.Api.Data;
using CarWash.Api.DTOs;
using CarWash.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarWash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public BookingsController(AppDbContext db) => _db = db;

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!;

    // Customer: create a booking
    [HttpPost]
    public async Task<ActionResult<CreateBookingResultDto>> Create(CreateBookingDto dto)
    {
        var service = await _db.Services.FindAsync(dto.ServiceId);
        if (service is null || !service.IsActive) return BadRequest("Service not found.");

        var locationName = dto.City.Trim();
        var locationAvailable = await _db.ServiceLocations.AnyAsync(location =>
            location.IsActive && location.Name.ToLower() == locationName.ToLower());
        if (!locationAvailable) return BadRequest("Selected service location is not available.");

        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(candidate =>
            candidate.Id == dto.VehicleId && candidate.UserId == CurrentUserId);
        if (vehicle is null) return BadRequest("Select a valid vehicle.");

      var booking = new Booking
{
    UserId = CurrentUserId,
    VehicleId = vehicle.Id,
    ServiceId = dto.ServiceId,
    ScheduledAt = DateTime.SpecifyKind(dto.ScheduledAt, DateTimeKind.Utc),
    Notes = dto.Notes,
    Address = dto.Address.Trim(),
    City = locationName,
    Pincode = dto.Pincode.Trim(),
    PhoneNumber = dto.PhoneNumber.Trim(),
    Status = BookingStatus.Pending
};
        _db.Bookings.Add(booking);
await _db.SaveChangesAsync();

return Ok(new CreateBookingResultDto(booking.Id, service.Name));
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

    // Admin: permanently delete a booking
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var booking = await _db.Bookings.FindAsync(id);
        if (booking is null) return NotFound();

        _db.Bookings.Remove(booking);
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
            b.Pincode,
            b.ScheduledAt.AddDays(30)
        );
    }
}
