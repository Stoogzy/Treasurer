using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treasurer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ClassifyMixedMembershipPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ;WITH MixedMembershipCandidates AS (
                    SELECT
                        candidate.Id AS BookingEventId,
                        booking.Id AS BookingId,
                        booking.BookingPayment -
                            SUM(
                                CASE
                                    WHEN otherBookingEvent.Id <> candidate.Id
                                        THEN COALESCE(
                                            otherBookingEvent.EventValue,
                                            0)
                                    ELSE 0
                                END) AS MembershipValue
                    FROM Bookings AS booking
                    INNER JOIN BookingEvents AS candidate
                        ON candidate.BookingId = booking.Id
                    INNER JOIN BookingEvents AS otherBookingEvent
                        ON otherBookingEvent.BookingId = booking.Id
                    WHERE booking.BookingPayment > 0
                        AND candidate.EventValue IS NULL
                        AND candidate.EventValueSource = 0
                        AND candidate.TicketName LIKE '%League player%'
                        AND (
                            SELECT COUNT(*)
                            FROM BookingEvents AS possibleCandidate
                            WHERE possibleCandidate.BookingId = booking.Id
                                AND possibleCandidate.EventValue IS NULL
                                AND possibleCandidate.EventValueSource = 0
                                AND possibleCandidate.TicketName
                                    LIKE '%League player%'
                        ) = 1
                        AND EXISTS (
                            SELECT 1
                            FROM BookingEvents AS knownBookingEvent
                            WHERE knownBookingEvent.BookingId = booking.Id
                                AND knownBookingEvent.Id <> candidate.Id
                                AND knownBookingEvent.EventValue IS NOT NULL
                        )
                    GROUP BY
                        candidate.Id,
                        booking.Id,
                        booking.BookingPayment
                    HAVING booking.BookingPayment -
                        SUM(
                            CASE
                                WHEN otherBookingEvent.Id <> candidate.Id
                                    THEN COALESCE(
                                        otherBookingEvent.EventValue,
                                        0)
                                ELSE 0
                            END) > 0
                )
                UPDATE bookingEvent
                SET
                    bookingEvent.EventValue =
                        candidate.MembershipValue,
                    bookingEvent.EventValueSource = 5
                FROM BookingEvents AS bookingEvent
                INNER JOIN MixedMembershipCandidates AS candidate
                    ON candidate.BookingEventId = bookingEvent.Id;

                UPDATE payment
                SET payment.PaymentCategory = 3
                FROM Payments AS payment
                WHERE EXISTS (
                    SELECT 1
                    FROM BookingEvents AS bookingEvent
                    WHERE bookingEvent.BookingId = payment.BookingId
                        AND bookingEvent.EventValueSource = 5
                        AND bookingEvent.EventValue > 0
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
                    bookingEvent.EventValue = NULL,
                    bookingEvent.EventValueSource = 0
                FROM BookingEvents AS bookingEvent
                WHERE bookingEvent.EventValueSource = 5
                    AND bookingEvent.EventValue > 0
                    AND EXISTS (
                        SELECT 1
                        FROM Payments AS payment
                        WHERE payment.BookingId =
                            bookingEvent.BookingId
                            AND payment.PaymentCategory = 3
                    );

                UPDATE Payments
                SET PaymentCategory = 1
                WHERE PaymentCategory = 3;
                """);
        }
    }
}
