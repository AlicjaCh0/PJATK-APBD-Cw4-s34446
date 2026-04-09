using System;

namespace LegacyRenewalApp;

public class PricingCalculator : IPricing
{
    public PricingResult Calculate(Customer customer, SubscriptionPlan plan, int seatCount, string paymentMethod, bool includePremiumSupport, bool useLoyaltyPoints, decimal baseAmount)
    {
        string notes = string.Empty;
 
        var discount = CalculateDiscount(seatCount, useLoyaltyPoints, customer, baseAmount, plan, out var subtotal, ref notes);
        var support = CalculateSupportFee(includePremiumSupport, plan.Code, ref notes);
        var payment = CalculatePaymentFee(paymentMethod, subtotal + support, ref notes);
        
        var tax = CalculateTax(customer, subtotal + support + payment, out var final, ref notes);

        return new PricingResult {
            DiscountAmount = discount,
            SubtotalAfterDiscount = subtotal,
            SupportFee = support,
            PaymentFee = payment,
            TaxAmount = tax,
            FinalAmount = final,
            Notes = notes
        };
    }

    private decimal CalculateDiscount(int seatCount, bool useLoyaltyPoints, Customer customer, decimal baseAmount, SubscriptionPlan plan, out decimal subtotalAfterDiscount, ref string notes)
    {
        decimal discountAmount = 0m;

        if (customer.Segment == "Silver") { discountAmount += baseAmount * 0.05m; notes += "silver discount; "; }
        else if (customer.Segment == "Gold") { discountAmount += baseAmount * 0.10m; notes += "gold discount; "; }
        else if (customer.Segment == "Platinum") { discountAmount += baseAmount * 0.15m; notes += "platinum discount; "; }
        else if (customer.Segment == "Education" && plan.IsEducationEligible) { discountAmount += baseAmount * 0.20m; notes += "education discount; "; }

        if (customer.YearsWithCompany >= 5) { discountAmount += baseAmount * 0.07m; notes += "long-term loyalty discount; "; }
        else if (customer.YearsWithCompany >= 2) { discountAmount += baseAmount * 0.03m; notes += "basic loyalty discount; "; }

        if (seatCount >= 50) { discountAmount += baseAmount * 0.12m; notes += "large team discount; "; }
        else if (seatCount >= 20) { discountAmount += baseAmount * 0.08m; notes += "medium team discount; "; }
        else if (seatCount >= 10) { discountAmount += baseAmount * 0.04m; notes += "small team discount; "; }

        if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
        {
            
            int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
            
            discountAmount += pointsToUse;
            notes += $"loyalty points used: {pointsToUse}; ";
            
        }

        subtotalAfterDiscount = baseAmount - discountAmount;
        
        if (subtotalAfterDiscount < 300m)
        {
            
            subtotalAfterDiscount = 300m;
            notes += "minimum discounted subtotal applied; ";
        }

        return discountAmount;
    }

    private decimal CalculateSupportFee(bool include, string planCode, ref string notes)
    {
        if (!include) return 0;
        notes += "premium support included; ";
        return planCode switch { "START" => 250m, "PRO" => 400m, "ENTERPRISE" => 700m, _ => 0 };
    }

    private decimal CalculatePaymentFee(string method, decimal totalAmount, ref string notes)
    {
        decimal fee = method switch
        {
            "CARD" => totalAmount * 0.02m,
            "BANK_TRANSFER" => totalAmount * 0.01m,
            "PAYPAL" => totalAmount * 0.035m,
            _ => 0
        };

        notes += method switch { "CARD" => "card payment fee; ", "BANK_TRANSFER" => "bank transfer fee; ", "PAYPAL" => "paypal fee; ", _ => "invoice payment; " };
        return fee;
    }

    private decimal CalculateTax(Customer customer, decimal taxBase, out decimal finalAmount, ref string notes)
    {
        decimal rate = customer.Country switch { "Poland" => 0.23m, "Germany" => 0.19m, "Czech Republic" => 0.21m, "Norway" => 0.25m, _ => 0.20m };
        decimal tax = taxBase * rate;
        finalAmount = taxBase + tax;

        if (finalAmount < 500m)
        {
            finalAmount = 500m;
            notes += "minimum invoice amount applied; ";
        }

        return tax;
    }
}