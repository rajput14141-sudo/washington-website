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

    [HttpGet("test-route")]
    public IActionResult TestRoute()
    {
        return Ok("Auth controller is working");
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(CustomerRegisterDto dto)
    {
        var mobileNumber = dto.PhoneNumber.Trim();
        if (mobileNumber.Length != 10 || mobileNumber.Any(character => !char.IsDigit(character)))
            return BadRequest(new[] { "Mobile number must contain exactly 10 digits." });

        if (!string.Equals(dto.Password, dto.ConfirmPassword, StringComparison.Ordinal))
            return BadRequest(new[] { "Password and confirmation password do not match." });

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
        var user = await _userManager.FindByNameAsync(mobileNumber)
            ?? await _userManager.Users.SingleOrDefaultAsync(candidate => candidate.PhoneNumber == mobileNumber);
        if (user is null) return Unauthorized("Invalid credentials");

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized("Invalid credentials");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles);

        return Ok(new AuthResponseDto(token, user.Email!, user.FullName, roles));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordDto dto,
        CancellationToken cancellationToken)
    {
        const string responseMessage = "If the email exists, a password reset link has been sent.";
        var user = await _userManager.FindByEmailAsync(dto.Email.Trim());

        // Always return the same response for unknown addresses to prevent account enumeration.
        if (user?.Email is null)
            return Ok(new { message = responseMessage });

        if (!_emailService.IsConfigured)
            return Problem(
                "Password reset email is not configured.",
                statusCode: StatusCodes.Status503ServiceUnavailable);

        // Prefer the requesting frontend's origin, with the configured URL as a fallback.
        var frontendBaseUrl = string.Empty;
        var originHeader = Request.Headers.Origin.ToString();
        var refererHeader = Request.Headers.Referer.ToString();

        if (!string.IsNullOrWhiteSpace(originHeader) &&
            Uri.TryCreate(originHeader, UriKind.Absolute, out var originUri) &&
            (originUri.Scheme == Uri.UriSchemeHttp || originUri.Scheme == Uri.UriSchemeHttps))
        {
            frontendBaseUrl = $"{originUri.Scheme}://{originUri.Authority}";
        }
        else if (!string.IsNullOrWhiteSpace(refererHeader) &&
                 Uri.TryCreate(refererHeader, UriKind.Absolute, out var refererUri) &&
                 (refererUri.Scheme == Uri.UriSchemeHttp || refererUri.Scheme == Uri.UriSchemeHttps))
        {
            frontendBaseUrl = $"{refererUri.Scheme}://{refererUri.Authority}";
        }
        else
        {
            frontendBaseUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/') ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(frontendBaseUrl) ||
            !Uri.TryCreate(frontendBaseUrl, UriKind.Absolute, out var frontendUri) ||
            (frontendUri.Scheme != Uri.UriSchemeHttp && frontendUri.Scheme != Uri.UriSchemeHttps))
        {
            return Problem(
                "Frontend:BaseUrl is not configured.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var cleanBaseUrl = frontendBaseUrl.Trim().TrimEnd('/');
        var cleanEmail = Uri.EscapeDataString(user.Email.Trim());
        var cleanToken = Uri.EscapeDataString(resetToken.Trim());
        var resetUrl = $"{cleanBaseUrl}/reset-password?email={cleanEmail}&token={cleanToken}";

        await _emailService.SendPasswordResetAsync(
            user.Email,
            resetUrl,
            cancellationToken);

        return Ok(new { message = responseMessage });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        if (!string.Equals(dto.NewPassword, dto.ConfirmPassword, StringComparison.Ordinal))
            return BadRequest(new[] { "Password and confirmation password do not match." });

        var user = await _userManager.FindByEmailAsync(dto.Email.Trim());
        if (user is null)
            return BadRequest(new[] { "The password reset link is invalid or expired." });

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(error => error.Description));

        return Ok(new { message = "Your password has been reset successfully. You can now log in." });
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
