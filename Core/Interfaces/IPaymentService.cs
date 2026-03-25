using InventoryEnterpriseProject.Core.Entities;

namespace InventoryEnterpriseProject.Core.Interfaces;

public interface IPaymentService
{
    Task<Payment> CreateRazorpayOrderAsync(decimal amount, string currency, string description);
    Task<Payment?> GetPaymentAsync(int id);
    IEnumerable<Payment> GetAllPayments();
    Task<bool> ConfirmPaymentAsync(string orderId, string paymentId, string signature);
    Task ClearPendingPaymentsAsync();
}
