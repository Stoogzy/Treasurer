using Treasurer.Core.Enums;

namespace Treasurer.Application.Common.Payments;

public static class PaymentClassification
{
    public const string MembershipSubscriptionMethod =
        "Membership subscription";

    public static PaymentCategory DetermineCategory(
        string paymentMethod,
        bool hasLinkedBooking,
        bool isEntirePaymentMembership,
        bool hasMembershipComponent)
    {
        if (string.Equals(
                paymentMethod,
                MembershipSubscriptionMethod,
                StringComparison.OrdinalIgnoreCase) ||
            isEntirePaymentMembership)
        {
            return PaymentCategory.Membership;
        }

        if (hasMembershipComponent)
        {
            return PaymentCategory.Mixed;
        }

        return hasLinkedBooking
            ? PaymentCategory.Event
            : PaymentCategory.Uncategorised;
    }
}
