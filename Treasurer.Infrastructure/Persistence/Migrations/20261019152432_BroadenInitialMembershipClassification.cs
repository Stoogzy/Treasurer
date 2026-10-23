using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treasurer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BroadenInitialMembershipClassification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE bookingEvent
                SET
                    bookingEvent.EventValue = 0,
                    bookingEvent.EventValueSource = 5
                FROM BookingEvents AS bookingEvent
                INNER JOIN Bookings AS booking
                    ON booking.Id = bookingEvent.BookingId
                WHERE booking.BookingPayment IN (20, 25)
                    AND NOT EXISTS (
                        SELECT 1
                        FROM BookingEvents AS otherBookingEvent
                        WHERE otherBookingEvent.BookingId = booking.Id
                            AND otherBookingEvent.EventValueSource
                                NOT IN (0, 2, 5)
                    );

                UPDATE Payments
                SET PaymentCategory = 2
                WHERE EXISTS (
                    SELECT 1
                    FROM BookingEvents AS bookingEvent
                    WHERE bookingEvent.BookingId = Payments.BookingId
                        AND bookingEvent.EventValueSource = 5
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE bookingEvent
                SET
                    bookingEvent.EventValue =
                        CASE
                            WHEN eventCount.BookingEventCount = 1
                                THEN booking.BookingPayment
                            ELSE NULL
                        END,
                    bookingEvent.EventValueSource =
                        CASE
                            WHEN eventCount.BookingEventCount = 1
                                THEN 2
                            ELSE 0
                        END
                FROM BookingEvents AS bookingEvent
                INNER JOIN Bookings AS booking
                    ON booking.Id = bookingEvent.BookingId
                INNER JOIN Events AS eventEntity
                    ON eventEntity.Id = bookingEvent.EventId
                INNER JOIN (
                    SELECT
                        BookingId,
                        COUNT(*) AS BookingEventCount
                    FROM BookingEvents
                    GROUP BY BookingId
                ) AS eventCount
                    ON eventCount.BookingId = booking.Id
                WHERE bookingEvent.EventValueSource = 5
                    AND eventEntity.EventName NOT LIKE '%Recreational%';

                UPDATE Payments
                SET PaymentCategory = 1
                WHERE PaymentMethod <> 'Membership subscription'
                    AND BookingId IS NOT NULL
                    AND NOT EXISTS (
                        SELECT 1
                        FROM BookingEvents AS bookingEvent
                        WHERE bookingEvent.BookingId = Payments.BookingId
                            AND bookingEvent.EventValueSource = 5
                    );
                """);
        }
    }
}
