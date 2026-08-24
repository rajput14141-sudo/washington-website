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
public class VehiclesController : ControllerBase
{
    private readonly AppDbContext _db;
    public VehiclesController(AppDbContext db) => _db = db;

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!;

    [HttpGet]
    public async Task<ActionResult<List<VehicleDto>>> GetMine()
    {
        var vehicles = await _db.Vehicles.Where(v => v.UserId == CurrentUserId)
            .Select(v => new VehicleDto(v.Id, v.Make, v.Model, v.LicensePlate, v.Type))
            .ToListAsync();
        return Ok(vehicles);
    }

    [HttpPost]
    public async Task<ActionResult<VehicleDto>> Create(CreateVehicleDto dto)
    {
        var vehicle = new Vehicle
        {
            Make = dto.Make,
            Model = dto.Model,
            LicensePlate = dto.LicensePlate,
            Type = dto.Type,
            UserId = CurrentUserId
        };
        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();
        return Ok(new VehicleDto(vehicle.Id, vehicle.Make, vehicle.Model, vehicle.LicensePlate, vehicle.Type));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.UserId == CurrentUserId);
        if (vehicle is null) return NotFound();
        _db.Vehicles.Remove(vehicle);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
