using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWash.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDefaultServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name", "Price", "PriceLabel" },
                values: new object[] { "Daily car cleaning service at your parking spot.", "Basic Car wash", 499m, "499" });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name", "Price", "PriceLabel" },
                values: new object[] { "Get your car fully cleaned at our service center. The package includes a complete exterior and interior wash for a clean and refreshed vehicle.", "Full car wash at Center", 199m, "199" });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name", "Price", "PriceLabel" },
                values: new object[] { "Includes car body polishing, mirror shining, and tyre polishing. Available twice a month to keep your car looking its best.", "Car Shine & Polishing Package", 99m, "99" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name", "Price", "PriceLabel" },
                values: new object[] { "Exterior wash & dry", "Basic Wash", 400m, "400" });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name", "Price", "PriceLabel" },
                values: new object[] { "Exterior + interior vacuum", "Deluxe Wash", 500m, "500" });

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name", "Price", "PriceLabel" },
                values: new object[] { "Complete interior & exterior detailing", "Full Detail", 400m, "400" });
        }
    }
}
