using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWash.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRazorpayAndRupeePrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StripePaymentIntentId",
                table: "Payments",
                newName: "RazorpayOrderId");

            migrationBuilder.AddColumn<string>(
                name: "RazorpayPaymentId",
                table: "Payments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.Sql("UPDATE Payments SET Currency = 'INR'");

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "Price",
                value: 400m);

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "Price",
                value: 500m);

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "Price",
                value: 400m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RazorpayPaymentId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "RazorpayOrderId",
                table: "Payments",
                newName: "StripePaymentIntentId");

            migrationBuilder.Sql("UPDATE Payments SET Currency = 'usd'");

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 1,
                column: "Price",
                value: 15m);

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 2,
                column: "Price",
                value: 30m);

            migrationBuilder.UpdateData(
                table: "Services",
                keyColumn: "Id",
                keyValue: 3,
                column: "Price",
                value: 90m);
        }
    }
}
