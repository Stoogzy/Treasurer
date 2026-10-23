using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treasurer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillMembershipIncludedRecreationalSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE bookingEvent
                SET
                    bookingEvent.EventValue = 0,
                    bookingEvent.EventValueSource = 4
                FROM BookingEvents AS bookingEvent
                INNER JOIN Bookings AS booking
                    ON booking.Id = bookingEvent.BookingId
                INNER JOIN Events AS eventEntity
                    ON eventEntity.Id = bookingEvent.EventId
                WHERE bookingEvent.EventValueSource = 0
                    AND bookingEvent.EventValue IS NULL
                    AND (
                        booking.BookingPayment IS NULL
                        OR booking.BookingPayment = 0
                    )
                    AND eventEntity.EventName LIKE '%Recreational%'
                    AND bookingEvent.TicketName LIKE '%League Player%';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE bookingEvent
                SET
                    bookingEvent.EventValue = NULL,
                    bookingEvent.EventValueSource = 0
                FROM BookingEvents AS bookingEvent
                INNER JOIN Bookings AS booking
                    ON booking.Id = bookingEvent.BookingId
                INNER JOIN Events AS eventEntity
                    ON eventEntity.Id = bookingEvent.EventId
                WHERE bookingEvent.EventValueSource = 4
                    AND bookingEvent.EventValue = 0
                    AND (
                        booking.BookingPayment IS NULL
                        OR booking.BookingPayment = 0
                    )
                    AND eventEntity.EventName LIKE '%Recreational%'
                    AND bookingEvent.TicketName LIKE '%League Player%';
                """);
        }
    }
}
