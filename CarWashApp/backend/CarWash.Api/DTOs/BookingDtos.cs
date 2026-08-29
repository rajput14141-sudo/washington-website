using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.DTOs;

public record VehicleDto(int Id, string Make, string Model, string LicensePlate, string Type);
public record CreateVehicleDto(string Make, string Model, string LicensePlate, string Type);

public record ServiceDto(int Id, string Name, string Description, decimal Price);

public record CustomerRegistrationDto(
    [Required, MaxLength(255)] string Name,
    [Required, RegularExpression(@"^[789]\d{9}$", ErrorMessage = "Mobile number must contain 10 digits and start with 7, 8, or 9.")] string Phone,
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MaxLength(500)] string Address
);

public record CreateBookingDto(
    int VehicleId,
    int ServiceId,
    DateTime ScheduledAt,
    string? Notes,
    [Required, MaxLength(300)] string Address,
    [Required, MaxLength(100)] string City,
    [Required, MaxLength(20)] string Pincode,
    [Required, RegularExpression(@"^[789]\d{9}$", ErrorMessage = "Mobile number must contain 10 digits and start with 7, 8, or 9.")] string PhoneNumber
);

public record CreateBookingResultDto(int Id, string ServiceName);

public record BookingDto(
    int Id,
    string CustomerName,
    VehicleDto Vehicle,
    ServiceDto Service,
    DateTime ScheduledAt,
    string Status,
    string? Notes,
    string Address,
    string City,
    string Pincode,
    DateTime ExpireDate
);

public record UpdateBookingStatusDto(string Status);
