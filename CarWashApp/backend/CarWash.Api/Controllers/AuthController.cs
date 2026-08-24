using CarWash.Api.DTOs;
using CarWash.Api.Models;
using CarWash.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Security.Cryptography;
using System.Text;

namespace CarWash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IAuthMirror _authMirror;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IAuthMirror authMirror,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _authMirror = authMirror;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.PhoneNumber) || string.IsNullOrWhiteSpace(dto.Address))
            return BadRequest(new[] { "Phone number and address are required." });

        var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, FullName = dto.FullName };
        var generatedPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)) + "aA1!";
        var result = await _userManager.CreateAsync(user, generatedPassword);

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Problem("Could not assign the customer role.", statusCode: StatusCodes.Status500InternalServerError);
        }

        try
        {
            await _authMirror.AddSignupAsync(
                user.Email!,
                user.FullName,
                dto.PhoneNumber.Trim(),
                dto.Address.Trim(),
                user.PasswordHash!,
                HttpContext.RequestAborted);
        }
        catch (MySqlException exception)
        {
            _logger.LogError(exception, "Could not mirror registration to MySQL for {Email}", user.Email);
            await _userManager.DeleteAsync(user);
            return Problem(
                "Could not save signup data to MySQL. Check the washin_ton.customer_signup table and connection.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (InvalidOperationException exception)
        {
            await _userManager.DeleteAsync(user);
            return Problem(exception.Message, statusCode: StatusCodes.Status500InternalServerError);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles);

        return Ok(new AuthResponseDto(token, user.Email!, user.FullName, roles));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null) return Unauthorized("Invalid credentials");

        MySqlSignupRecord? signupRecord;
        try
        {
            signupRecord = await _authMirror.FindSignupAsync(dto.Email, HttpContext.RequestAborted);
        }
        catch (MySqlException exception)
        {
            _logger.LogError(exception, "Could not fetch MySQL signup data for {Email}", dto.Email);
            return Problem(
                "Could not read signup data from MySQL.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        if (signupRecord is null || !CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(signupRecord.PasswordHash),
            Encoding.UTF8.GetBytes(dto.Password.Trim())))
            return Unauthorized("Invalid credentials");

        var roles = await _userManager.GetRolesAsync(user);

        try
        {
            await _authMirror.AddLoginAsync(
                user.Email!,
                user.FullName,
                user.PasswordHash!,
                HttpContext.RequestAborted);
        }
        catch (MySqlException exception)
        {
            _logger.LogError(exception, "Could not mirror login to MySQL for {Email}", user.Email);
            return Problem(
                "Login was verified, but its MySQL record could not be saved. Check the washin_ton.login table.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var token = _tokenService.CreateToken(user, roles);

        return Ok(new AuthResponseDto(token, user.Email!, user.FullName, roles));
    }

    [HttpPost("admin/login")]
    public async Task<ActionResult<AuthResponseDto>> LoginAdmin(LoginDto dto)
    {
        var signupRecord = await _authMirror.FindAdminSignupAsync(dto.Email, HttpContext.RequestAborted);
        if (signupRecord is null)
            return Unauthorized("Invalid admin credentials");

        var user = await _userManager.FindByEmailAsync(dto.Email);
        var suppliedPassword = Encoding.UTF8.GetBytes(dto.Password);
        var storedPassword = Encoding.UTF8.GetBytes(signupRecord.PasswordHash);
        var passwordMatches = suppliedPassword.Length == storedPassword.Length &&
            CryptographicOperations.FixedTimeEquals(suppliedPassword, storedPassword);

        if (!passwordMatches && user is not null)
            try
            {
                passwordMatches = _userManager.PasswordHasher.VerifyHashedPassword(
                    user, signupRecord.PasswordHash, dto.Password) != PasswordVerificationResult.Failed;
            }
            catch (FormatException)
            {
                passwordMatches = false;
            }

        if (!passwordMatches)
            return Unauthorized("Invalid admin credentials");

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = signupRecord.Email,
                Email = signupRecord.Email,
                FullName = signupRecord.Name
            };
            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                return Problem("Could not initialize the admin account.", statusCode: StatusCodes.Status500InternalServerError);
        }

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
            if (!roleResult.Succeeded)
                return Problem("Could not assign the admin role.", statusCode: StatusCodes.Status500InternalServerError);
        }

        try
        {
            await _authMirror.AddAdminLoginAsync(
                user.Email!, user.FullName, user.PasswordHash!, HttpContext.RequestAborted);
        }
        catch (MySqlException exception)
        {
            _logger.LogError(exception, "Could not mirror admin login for {Email}", user.Email);
            return Problem("Could not save admin login data to MySQL.", statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles);
        return Ok(new AuthResponseDto(token, user.Email!, user.FullName, roles));
    }

    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAdmin(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new[] { "Password is required." });

        if (dto.Password != dto.ConfirmPassword)
            return BadRequest(new[] { "Password and confirm password must match." });

        var email = dto.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);
        var createdUser = user is null;
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = dto.FullName.Trim()
            };
            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                return BadRequest(createResult.Errors.Select(error => error.Description));
        }
        else
        {
            if (!await _userManager.CheckPasswordAsync(user, dto.Password))
                return BadRequest(new[] { "This email already belongs to an account with a different password." });

            user.FullName = dto.FullName.Trim();
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return BadRequest(updateResult.Errors.Select(error => error.Description));
        }

        var alreadyAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        var roleResult = alreadyAdmin
            ? IdentityResult.Success
            : await _userManager.AddToRoleAsync(user, "Admin");
        if (!roleResult.Succeeded)
        {
            if (createdUser)
                await _userManager.DeleteAsync(user);
            return Problem("Could not assign the admin role.", statusCode: StatusCodes.Status500InternalServerError);
        }

        try
        {
            await _authMirror.AddAdminSignupAsync(
                user.Email!, user.FullName, user.PasswordHash!, HttpContext.RequestAborted);
        }
        catch (MySqlException exception)
        {
            _logger.LogError(exception, "Could not save new admin {Email} to MySQL", user.Email);
            if (createdUser)
                await _userManager.DeleteAsync(user);
            else if (!alreadyAdmin)
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            return Problem(
                "Could not save the admin to the admin_signup table.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return Created(string.Empty, new { user.Email, user.FullName });
    }
}
