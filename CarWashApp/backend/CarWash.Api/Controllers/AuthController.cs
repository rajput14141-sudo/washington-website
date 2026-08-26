using CarWash.Api.DTOs;
using CarWash.Api.Models;
using CarWash.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarWash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(CustomerRegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
            return BadRequest(new[] { "Name is required." });

        if (string.IsNullOrWhiteSpace(dto.Address))
            return BadRequest(new[] { "Address is required." });

        if (dto.Password != dto.ConfirmPassword)
            return BadRequest(new[] { "Password and confirm password do not match." });

        var mobileNumber = dto.PhoneNumber.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _userManager.FindByEmailAsync(email) is not null)
            return BadRequest(new[] { "An account with this email already exists." });

        if (await _userManager.Users.AnyAsync(user => user.PhoneNumber == mobileNumber))
            return BadRequest(new[] { "An account with this mobile number already exists." });

        var user = new ApplicationUser
        {
            UserName = mobileNumber,
            Email = email,
            FullName = dto.FullName.Trim(),
            PhoneNumber = mobileNumber,
            Address = dto.Address.Trim()
        };
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Problem("Could not assign the customer role.", statusCode: StatusCodes.Status500InternalServerError);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles);

        return Ok(new AuthResponseDto(token, user.Email!, user.FullName, roles));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(CustomerLoginDto dto)
    {
        var mobileNumber = dto.PhoneNumber.Trim();
        if (mobileNumber.Length != 10 || mobileNumber.Any(character => !char.IsDigit(character)))
            return Unauthorized("Invalid mobile number or password");

        var user = await _userManager.Users
            .SingleOrDefaultAsync(candidate => candidate.PhoneNumber == mobileNumber);
        if (user is null) return Unauthorized("Invalid credentials");

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized("Invalid credentials");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles);

        return Ok(new AuthResponseDto(token, user.Email!, user.FullName, roles));
    }

    [HttpPost("admin/register")]
    public async Task<ActionResult<AuthResponseDto>> RegisterAdmin(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new[] { "Password is required." });

        var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, FullName = dto.FullName };
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(error => error.Description));

        var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Problem("Could not assign the admin role.", statusCode: StatusCodes.Status500InternalServerError);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles);
        return Ok(new AuthResponseDto(token, user.Email!, user.FullName, roles));
    }

    [HttpPost("admin/login")]
    public async Task<ActionResult<AuthResponseDto>> LoginAdmin(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !await _userManager.IsInRoleAsync(user, "Admin"))
            return Unauthorized("Invalid admin credentials");

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized("Invalid admin credentials");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles);
        return Ok(new AuthResponseDto(token, user.Email!, user.FullName, roles));
    }
}
