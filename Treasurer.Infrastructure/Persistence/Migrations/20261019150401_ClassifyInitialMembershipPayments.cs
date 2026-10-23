using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treasurer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ClassifyInitialMembershipPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentCategory",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE Payments
                SET PaymentCategory = 1
                WHERE BookingId IS NOT NULL;

                UPDATE bookingEvent
                SET
                    bookingEvent.EventValue = 0,
                    bookingEvent.EventValueSource = 5
                FROM BookingEvents AS bookingEvent
                INNER JOIN Bookings AS booking
                    ON booking.Id = bookingEvent.BookingId
                INNER JOIN Events AS eventEntity
                    ON eventEntity.Id = bookingEvent.EventId
                WHERE bookingEvent.EventValueSource = 2
                    AND booking.BookingPayment > 0
                    AND eventEntity.EventName LIKE '%Recreational%'
                    AND bookingEvent.TicketName LIKE '%League Player%';

                UPDATE Payments
                SET PaymentCategory = 2
                WHERE PaymentMethod = 'Membership subscription'
                    OR EXISTS (
                        SELECT 1
                        FROM BookingEvents AS bookingEvent
                        WHERE bookingEvent.BookingId = Payments.BookingId
                            AND bookingEvent.EventValueSource = 5
                    );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentCategory",
                table: "Payments",
                column: "PaymentCategory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE BookingEvents
                SET
                    EventValue = NULL,
                    EventValueSource = 0
                WHERE EventValueSource = 5;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Payments_PaymentCategory",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentCategory",
                table: "Payments");
        }
    }
}
