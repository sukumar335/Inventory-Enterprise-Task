using InventoryEnterpriseProject.Core.Entities;

namespace InventoryEnterpriseProject.Core.Interfaces;

public interface IPaymentService
{
    Task<Payment> CreatePaymentIntentAsync(decimal amount, string currency, string description);
    Task<Payment?> GetPaymentAsync(int id);
    IEnumerable<Payment> GetAllPayments();
    Task<bool> ConfirmPaymentAsync(string paymentIntentId);
}
