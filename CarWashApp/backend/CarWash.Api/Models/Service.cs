namespace CarWash.Api.Models;

public class Service
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // e.g. "Basic Wash", "Full Detail"
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string PriceLabel { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
}
