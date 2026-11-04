using Treasurer.Application.Common.Imports.Assumptions;
using Treasurer.Core.Enums;

namespace Treasurer.Application.Common.Payments;

public static class PaymentClassification
{
    public static PaymentCategory DetermineCategory(
        string paymentMethod,
        bool hasLinkedBooking,
        bool isEntirePaymentMembership,
        bool hasMembershipComponent)
    {
        if (MembershipSubscriptionPaymentAssumption.Applies(paymentMethod) ||
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
