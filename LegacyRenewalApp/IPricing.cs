namespace LegacyRenewalApp;

public interface IPricing
{
    PricingResult Calculate(Customer customer, SubscriptionPlan plan, int seatCount,
        string paymentMethod, bool includePremiumSupport, bool useLoyaltyPoints, decimal baseAmount);
}