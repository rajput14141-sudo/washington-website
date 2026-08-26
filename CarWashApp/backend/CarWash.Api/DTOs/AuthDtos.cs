using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.DTOs;

public record CustomerRegisterDto(
	[Required, MaxLength(100)] string FullName,
	[Required, EmailAddress, MaxLength(256)] string Email,
	[Required, RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must contain exactly 10 digits.")]
	string PhoneNumber,
	[Required, MaxLength(500)] string Address,
	[Required, MinLength(8), MaxLength(100)] string Password,
	[Required] string ConfirmPassword);

public record RegisterDto(
	string FullName,
	string Email,
	string? Password = null,
	string? ConfirmPassword = null,
	string? PhoneNumber = null,
	string? Address = null);
public record LoginDto(string Email, string Password);
public record CustomerLoginDto(
	[Required, RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must contain exactly 10 digits.")]
	string PhoneNumber,
	[Required] string Password);
public record AuthResponseDto(string Token, string Email, string FullName, IList<string> Roles);
public record CustomerDetailsDto(string Id, string FullName, string Email, string? PhoneNumber, string Address);
