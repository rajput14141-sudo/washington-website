using CarWash.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarWash.Api.Controllers;

[ApiController]
[Route("api/site-settings")]
public class SiteSettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SiteSettingsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<SiteSettingsDto>> Get()
    {
        var settings = await _db.SiteSettings.AsNoTracking().SingleAsync(setting => setting.Id == 1);
        return Ok(ToDto(settings));
    }

    [HttpPut("rating")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SiteSettingsDto>> UpdateRating(UpdateRatingDto dto)
    {
        if (dto.Rating is < 0 or > 5)
            return BadRequest("Rating must be between 0 and 5.");

        var settings = await _db.SiteSettings.SingleAsync(setting => setting.Id == 1);
        settings.Rating = decimal.Round(dto.Rating, 1);
        await _db.SaveChangesAsync();

        return Ok(ToDto(settings));
    }

    [HttpPut("people-count")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SiteSettingsDto>> UpdatePeopleCount(UpdatePeopleCountDto dto)
    {
        if (dto.PeopleCount < 0)
            return BadRequest("People count cannot be negative.");

        var settings = await _db.SiteSettings.SingleAsync(setting => setting.Id == 1);
        settings.PeopleCount = dto.PeopleCount;
        await _db.SaveChangesAsync();

        return Ok(ToDto(settings));
    }

    private static SiteSettingsDto ToDto(CarWash.Api.Models.SiteSetting settings) =>
        new(settings.Rating, settings.PeopleCount);
}

public record SiteSettingsDto(decimal Rating, int PeopleCount);
public record UpdateRatingDto(decimal Rating);
public record UpdatePeopleCountDto(int PeopleCount);