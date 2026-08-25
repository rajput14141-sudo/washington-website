using CarWash.Api.DTOs;
using CarWash.Api.Models;
using CarWash.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.PhoneNumber) ||
            string.IsNullOrWhiteSpace(dto.Address))
        {
            return BadRequest(new[]
            {
                "Phone number and address are required."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new[]
            {
                "Password is required."
            });
        }

        if (dto.Password != dto.ConfirmPassword)
        {
            return BadRequest(new[]
            {
                "Password and confirm password must match."
            });
        }

        var email = dto.Email.Trim();

        var existingUser =
            await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            return BadRequest(new[]
            {
                "An account with this email already exists."
            });
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = dto.FullName.Trim(),
            PhoneNumber = dto.PhoneNumber.Trim(),
            Address = dto.Address.Trim()
        };

        var result =
            await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(
                result.Errors.Select(error => error.Description));
        }

        var roleResult =
            await _userManager.AddToRoleAsync(user, "Customer");

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            return Problem(
                "Could not assign the customer role.",
                statusCode:
                    StatusCodes.Status500InternalServerError);
        }

        var roles =
            await _userManager.GetRolesAsync(user);

        var token =
            _tokenService.CreateToken(user, roles);

        return Ok(
            new AuthResponseDto(
                token,
                user.Email!,
                user.FullName,
                roles));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(
        LoginDto dto)
    {
        var email = dto.Email.Trim();

        var user =
            await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return Unauthorized("Invalid credentials");
        }

        var passwordIsCorrect =
            await _userManager.CheckPasswordAsync(
                user,
                dto.Password);

        if (!passwordIsCorrect)
        {
            return Unauthorized("Invalid credentials");
        }

        var roles =
            await _userManager.GetRolesAsync(user);

        var token =
            _tokenService.CreateToken(user, roles);

        return Ok(
            new AuthResponseDto(
                token,
                user.Email!,
                user.FullName,
                roles));
    }

    [HttpPost("admin/login")]
    public async Task<ActionResult<AuthResponseDto>> LoginAdmin(
        LoginDto dto)
    {
        var email = dto.Email.Trim();

        var user =
            await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return Unauthorized("Invalid admin credentials");
        }

        var passwordIsCorrect =
            await _userManager.CheckPasswordAsync(
                user,
                dto.Password);

        if (!passwordIsCorrect)
        {
            return Unauthorized("Invalid admin credentials");
        }

        var isAdmin =
            await _userManager.IsInRoleAsync(user, "Admin");

        if (!isAdmin)
        {
            return Unauthorized("Invalid admin credentials");
        }

        var roles =
            await _userManager.GetRolesAsync(user);

        var token =
            _tokenService.CreateToken(user, roles);

        return Ok(
            new AuthResponseDto(
                token,
                user.Email!,
                user.FullName,
                roles));
    }

    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAdmin(
        RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new[]
            {
                "Password is required."
            });
        }

        if (dto.Password != dto.ConfirmPassword)
        {
            return BadRequest(new[]
            {
                "Password and confirm password must match."
            });
        }

        var email = dto.Email.Trim();

        var user =
            await _userManager.FindByEmailAsync(email);

        var createdUser = user is null;

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = dto.FullName.Trim(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                Address = dto.Address?.Trim() ?? string.Empty
            };

            var createResult =
                await _userManager.CreateAsync(
                    user,
                    dto.Password);

            if (!createResult.Succeeded)
            {
                return BadRequest(
                    createResult.Errors.Select(
                        error => error.Description));
            }
        }
        else
        {
            var passwordIsCorrect =
                await _userManager.CheckPasswordAsync(
                    user,
                    dto.Password);

            if (!passwordIsCorrect)
            {
                return BadRequest(new[]
                {
                    "This email already belongs to an account with a different password."
                });
            }

            user.FullName = dto.FullName.Trim();
            user.PhoneNumber = dto.PhoneNumber?.Trim();
            user.Address = dto.Address?.Trim() ?? user.Address;

            var updateResult =
                await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return BadRequest(
                    updateResult.Errors.Select(
                        error => error.Description));
            }
        }

        var alreadyAdmin =
            await _userManager.IsInRoleAsync(user, "Admin");

        if (!alreadyAdmin)
        {
            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    "Admin");

            if (!roleResult.Succeeded)
            {
                if (createdUser)
                {
                    await _userManager.DeleteAsync(user);
                }

                return Problem(
                    "Could not assign the admin role.",
                    statusCode:
                        StatusCodes.Status500InternalServerError);
            }
        }

        return Created(
            string.Empty,
            new
            {
                user.Email,
                user.FullName
            });
    }
}