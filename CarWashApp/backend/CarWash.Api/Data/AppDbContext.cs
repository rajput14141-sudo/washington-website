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
            new Service { Id = 1, Name = "Basic Wash", Description = "Exterior wash & dry", Price = 400, PriceLabel = "400" },
            new Service { Id = 2, Name = "Deluxe Wash", Description = "Exterior + interior vacuum", Price = 500, PriceLabel = "500" },
            new Service { Id = 3, Name = "Full Detail", Description = "Complete interior & exterior detailing", Price = 400, PriceLabel = "400" }
        );

        builder.Entity<SiteSetting>().HasData(new SiteSetting { Id = 1, Rating = 4.8m });

        builder.Entity<ApplicationUser>()
            .HasIndex(user => user.PhoneNumber)
            .IsUnique();
    }
}
