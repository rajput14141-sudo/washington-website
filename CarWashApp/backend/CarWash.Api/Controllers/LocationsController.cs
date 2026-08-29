using CarWash.Api.Data;
using CarWash.Api.DTOs;
using CarWash.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarWash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public LocationsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<ServiceLocationDto>>> GetAll()
    {
        var locations = await _db.ServiceLocations
            .Where(location => location.IsActive)
            .OrderBy(location => location.Name)
            .Select(location => new ServiceLocationDto(location.Id, location.Name))
            .ToListAsync();

        return Ok(locations);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServiceLocationDto>> Create(SaveServiceLocationDto dto)
    {
        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Location name is required.");

        var duplicate = await _db.ServiceLocations.AnyAsync(location =>
            location.IsActive && location.Name.ToLower() == name.ToLower());
        if (duplicate) return Conflict("This location already exists.");

        var location = new ServiceLocation { Name = name };
        _db.ServiceLocations.Add(location);
        await _db.SaveChangesAsync();

        return Ok(new ServiceLocationDto(location.Id, location.Name));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, SaveServiceLocationDto dto)
    {
        var location = await _db.ServiceLocations.FindAsync(id);
        if (location is null || !location.IsActive) return NotFound();

        var name = dto.Name.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Location name is required.");

        var duplicate = await _db.ServiceLocations.AnyAsync(candidate =>
            candidate.Id != id && candidate.IsActive && candidate.Name.ToLower() == name.ToLower());
        if (duplicate) return Conflict("This location already exists.");

        location.Name = name;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var location = await _db.ServiceLocations.FindAsync(id);
        if (location is null) return NotFound();

        location.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
