namespace CarWash.Api.DTOs;

public record RegisterDto(
	string FullName,
	string Email,
	string? Password = null,
	string? ConfirmPassword = null,
	string? PhoneNumber = null,
	string? Address = null);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string Token, string Email, string FullName, IList<string> Roles);
