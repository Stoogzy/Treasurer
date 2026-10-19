using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treasurer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attendees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttendeeCustomerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AttendeeCustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bookers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookerEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookwhenEventId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    EventCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StartsAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true),
                    EndsAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookwhenBookingId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BookingReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BookingPayment = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BookerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Bookers_BookerId",
                        column: x => x.BookerId,
                        principalTable: "Bookers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookingEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookwhenEntryId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttendeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TicketType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Attended = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingEvents_Attendees_AttendeeId",
                        column: x => x.AttendeeId,
                        principalTable: "Attendees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingEvents_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingEvents_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    BookingReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PayerEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendees_AttendeeCustomerId",
                table: "Attendees",
                column: "AttendeeCustomerId",
                unique: true,
                filter: "[AttendeeCustomerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Bookers_BookerEmail",
                table: "Bookers",
                column: "BookerEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingEvents_AttendeeId",
                table: "BookingEvents",
                column: "AttendeeId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingEvents_BookingId",
                table: "BookingEvents",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingEvents_BookingId_EventId",
                table: "BookingEvents",
                columns: new[] { "BookingId", "EventId" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingEvents_BookwhenEntryId",
                table: "BookingEvents",
                column: "BookwhenEntryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingEvents_EventId",
                table: "BookingEvents",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookerId",
                table: "Bookings",
                column: "BookerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookingReference",
                table: "Bookings",
                column: "BookingReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BookwhenBookingId",
                table: "Bookings",
                column: "BookwhenBookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_BookwhenEventId",
                table: "Events",
                column: "BookwhenEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventCode",
                table: "Events",
                column: "EventCode");

            migrationBuilder.CreateIndex(
                name: "IX_Events_StartsAt",
                table: "Events",
                column: "StartsAt");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId",
                table: "Payments",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingReference",
                table: "Payments",
                column: "BookingReference");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentDate",
                table: "Payments",
                column: "PaymentDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingEvents");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Attendees");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Bookers");
        }
    }
}
