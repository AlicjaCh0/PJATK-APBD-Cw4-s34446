namespace LegacyRenewalApp;

public class PricingResult
{
    public decimal DiscountAmount { get; set; }
    public decimal SubtotalAfterDiscount { get; set; }
    public decimal SupportFee { get; set; }
    public decimal PaymentFee { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public string Notes { get; set; }
}