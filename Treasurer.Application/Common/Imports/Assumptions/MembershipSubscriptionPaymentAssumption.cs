namespace Treasurer.Application.Common.Imports.Assumptions;

/// <summary>
/// Assumption 5: A payment whose method is "Membership subscription" is a recurring
/// membership payment, even when it has no matching booking in the Events CSV.
/// </summary>
internal static class MembershipSubscriptionPaymentAssumption
{
    public const string MembershipSubscriptionMethod = "Membership subscription";

    public static bool Applies(string paymentMethod)
    {
        return string.Equals(
            paymentMethod,
            MembershipSubscriptionMethod,
            StringComparison.OrdinalIgnoreCase);
    }
}
