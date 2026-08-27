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
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _configuration = configuration;
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

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        if (!_emailService.IsConfigured)
            return Problem("Password reset email is not configured.", statusCode: StatusCodes.Status503ServiceUnavailable);

        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(email);
        if (user is not null && await _userManager.IsInRoleAsync(user, "Customer"))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var frontendBaseUrl = (_configuration["Frontend:BaseUrl"] ?? "http://localhost:5173").TrimEnd('/');
            var resetUrl = $"{frontendBaseUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
           try
{
   try
{
    try
{
   try
{
    await _emailService.SendPasswordResetAsync(email, resetUrl);
}
catch (Exception ex)
{
    return BadRequest(ex.ToString());
}
}
catch (Exception ex)
{
    return BadRequest(ex.ToString());
}
}
catch (Exception ex)
{
    return BadRequest(ex.Message);
}
}
catch (Exception ex)
{
    return BadRequest(ex.ToString());
}
        }

        return Ok(new { message = "If the email is registered, a password reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
            return BadRequest(new[] { "Password and confirm password do not match." });

        var user = await _userManager.FindByEmailAsync(dto.Email.Trim().ToLowerInvariant());
        if (user is null || !await _userManager.IsInRoleAsync(user, "Customer"))
            return BadRequest(new[] { "Invalid password reset request." });

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(error => error.Description));

        return Ok(new { message = "Password reset successful. You can now log in with your mobile number." });
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
