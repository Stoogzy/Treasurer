using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treasurer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PersistBookwhenEventValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "BookingPayment",
                table: "Bookings",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<bool>(
                name: "IsGuardianBooking",
                table: "Bookings",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EventValue",
                table: "BookingEvents",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EventValueSource",
                table: "BookingEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGuardianBooking",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "EventValue",
                table: "BookingEvents");

            migrationBuilder.DropColumn(
                name: "EventValueSource",
                table: "BookingEvents");

            migrationBuilder.AlterColumn<decimal>(
                name: "BookingPayment",
                table: "Bookings",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);
        }
    }
}
