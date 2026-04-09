namespace LegacyRenewalApp;

public class LegacyBillingService : IBillService
{
    
    
    public void SendNotification(string customerEmail,string customerName, string planCode, decimal amount)
    {
        string tyt = "Faktura za odnowienie subskrybcji";
        
        string body = $"Dzien dobry {customerName}, faktura za twoją umowę {planCode} jest gotowa. Wartosc wynosi: {amount:F2}.";
        LegacyBillingGateway.SendEmail(customerEmail, tyt, body);
        
    }
    
    
    public void SaveInvoice(RenewalInvoice invoice) => LegacyBillingGateway.SaveInvoice(invoice);
    
    
}