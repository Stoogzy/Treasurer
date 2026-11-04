using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treasurer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGuardianBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGuardianBooking",
                table: "Bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGuardianBooking",
                table: "Bookings",
                type: "bit",
                nullable: true);
        }
    }
}
