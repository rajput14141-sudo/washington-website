namespace CarWash.Api.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string Type { get; set; } = "Sedan"; // Sedan, SUV, Truck, etc.

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
}
