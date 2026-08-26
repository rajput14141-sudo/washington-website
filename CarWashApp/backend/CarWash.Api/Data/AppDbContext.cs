using CarWash.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarWash.Api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Service>().HasData(
            new Service { Id = 1, Name = "Basic Car wash", Description = "Daily car cleaning service at your parking spot.", Price = 499, PriceLabel = "499" },
            new Service { Id = 2, Name = "Full car wash at Center", Description = "Get your car fully cleaned at our service center. The package includes a complete exterior and interior wash for a clean and refreshed vehicle.", Price = 199, PriceLabel = "199" },
            new Service { Id = 3, Name = "Car Shine & Polishing Package", Description = "Includes car body polishing, mirror shining, and tyre polishing. Available twice a month to keep your car looking its best.", Price = 99, PriceLabel = "99" }
        );

        builder.Entity<SiteSetting>().HasData(new SiteSetting { Id = 1, Rating = 4.8m });

        builder.Entity<ApplicationUser>()
            .HasIndex(user => user.PhoneNumber)
            .IsUnique();
    }
}
