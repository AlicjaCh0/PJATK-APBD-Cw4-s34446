using System;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
                
        private readonly ICustomerRepository _customerRepository;
        private readonly ISubscriptionPlanRepository _planRepository;
        private readonly IBillService _billService;
        private readonly IPricing _pricing;
        
        public SubscriptionRenewalService(
            
            ICustomerRepository customerRepository, 
            ISubscriptionPlanRepository planRepository, 
            IBillService billingService,
            IPricing pricing)
        {
            _customerRepository = customerRepository;
            _planRepository = planRepository;
            _billService = billingService;
            _pricing = pricing;
        }

        public SubscriptionRenewalService() : this(
            new CustomerRepository(), 
            new SubscriptionPlanRepository(), 
            new LegacyBillingService(),
            new PricingCalculator())
        {
        }
        public RenewalInvoice CreateRenewalInvoice(
            
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints
            )
        {
            
            
            InputsCheck(customerId, planCode, seatCount, paymentMethod);

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();
            

            var customer = _customerRepository.GetById(customerId);
            var plan = _planRepository.GetByCode(normalizedPlanCode);

            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }

            decimal baseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
            
            var p = _pricing.Calculate(customer, plan, seatCount, normalizedPaymentMethod, includePremiumSupport, useLoyaltyPoints, baseAmount);

            var invoice = new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{normalizedPlanCode}",
                CustomerName = customer.FullName,
                PlanCode = normalizedPlanCode,
                PaymentMethod = normalizedPaymentMethod,
                SeatCount = seatCount,
                BaseAmount = Math.Round(baseAmount, 2, MidpointRounding.AwayFromZero),
                DiscountAmount = Math.Round(p.DiscountAmount, 2, MidpointRounding.AwayFromZero),
                SupportFee = Math.Round(p.SupportFee, 2, MidpointRounding.AwayFromZero),
                PaymentFee = Math.Round(p.PaymentFee, 2, MidpointRounding.AwayFromZero),
                TaxAmount = Math.Round(p.TaxAmount, 2, MidpointRounding.AwayFromZero),
                FinalAmount = Math.Round(p.FinalAmount, 2, MidpointRounding.AwayFromZero),
                Notes = p.Notes.Trim(),
                GeneratedAt = DateTime.UtcNow
            };
            
            _billService.SaveInvoice(invoice);

            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                _billService.SendNotification(customer.Email, customer.FullName, normalizedPlanCode, invoice.FinalAmount);
            }

            return invoice;
        }
        
        
        
        private static void InputsCheck(int customerId, string planCode, int seatCount, string paymentMethod)
        {
            if (customerId <= 0)
            {
                throw new ArgumentException("Customer id must be positive");
            }

            if (string.IsNullOrWhiteSpace(planCode))
            {
                throw new ArgumentException("Plan code is required");
            }

            if (seatCount <= 0)
            {
                throw new ArgumentException("Seat count must be positive");
            }

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                throw new ArgumentException("Payment method is required");
            }
        }
    }
}
