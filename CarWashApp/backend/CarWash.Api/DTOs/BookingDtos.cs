using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.DTOs;

public record VehicleDto(int Id, string Make, string Model, string LicensePlate, string Type);
public record CreateVehicleDto(string Make, string Model, string LicensePlate, string Type);

public record ServiceDto(int Id, string Name, string Description, decimal Price);

public record CustomerRegistrationDto(
    [Required, MaxLength(255)] string Name,
    [Required, MaxLength(30)] string Phone,
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MaxLength(500)] string Address
);

public record CreateBookingDto(
    int ServiceId,
    [Required, MaxLength(255)] string CustomerName,
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MaxLength(30)] string Phone,
    [Required, MaxLength(100)] string VehicleMake,
    [Required, MaxLength(100)] string VehicleModel,
    [Required, MaxLength(30)] string LicensePlate,
    [Required, MaxLength(30)] string VehicleType,
    DateTime ScheduledAt,
    string? Notes,
    [Required, MaxLength(300)] string Address,
    [Required, MaxLength(100)] string City,
    [Required, MaxLength(20)] string Pincode
);

public record PublicBookingResultDto(BookingDto Booking, string AccessKey);

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
    string Pincode
);

public record UpdateBookingStatusDto(string Status);
