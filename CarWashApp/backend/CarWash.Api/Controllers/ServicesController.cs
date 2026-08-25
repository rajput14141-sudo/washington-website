using CarWash.Api.Data;
using CarWash.Api.DTOs;
using CarWash.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarWash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ServicesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<ServiceDto>>> GetAll()
    {
        var services = await _db.Services.Where(s => s.IsActive)
            .Select(s => new ServiceDto(s.Id, s.Name, s.Description, s.Price))
            .ToListAsync();
        return Ok(services);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServiceDto>> Create(ServiceDto dto)
    {
        var service = new Service
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price
        };
        _db.Services.Add(service);
        await _db.SaveChangesAsync();
        return Ok(new ServiceDto(service.Id, service.Name, service.Description, service.Price));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, ServiceDto dto)
    {
        var service = await _db.Services.FindAsync(id);
        if (service is null) return NotFound();

        service.Name = dto.Name;
        service.Description = dto.Description;
        service.Price = dto.Price;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await _db.Services.FindAsync(id);
        if (service is null) return NotFound();
        service.IsActive = false; // soft delete
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
