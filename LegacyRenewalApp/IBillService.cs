namespace LegacyRenewalApp;

public interface IBillService
{
    void SaveInvoice(RenewalInvoice r);
    
    void SendNotification(string email, string customerName, string planCode, decimal amount);
    
}