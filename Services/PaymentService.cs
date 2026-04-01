using Razorpay.Api;
using InventoryEnterpriseProject.Core.Interfaces;
using InventoryEnterpriseProject.Core.Entities;
using InventoryEnterpriseProject.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryEnterpriseProject.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public PaymentService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<Core.Entities.Payment> CreateRazorpayOrderAsync(decimal amount, string currency, string description)
    {
        var keyId = _configuration["Razorpay:KeyId"];
        var keySecret = _configuration["Razorpay:KeySecret"];
        
        RazorpayClient client = new RazorpayClient(keyId, keySecret);

        Dictionary<string, object> options = new Dictionary<string, object>
        {
            { "amount", (long)(amount * 100) }, // amount in the smallest currency unit (e.g., paise)
            { "currency", currency },
            { "receipt", Guid.NewGuid().ToString().Substring(0, 20) }
        };

        Order order = client.Order.Create(options);

        var payment = new Core.Entities.Payment
        {
            Description = description,
            Amount = amount,
            Currency = currency,
            Status = "Pending",
            RazorpayOrderId = order["id"].ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return payment;
    }

    public async Task<Core.Entities.Payment?> GetPaymentAsync(int id)
    {
        return await _context.Payments.FindAsync(id);
    }

    public IEnumerable<Core.Entities.Payment> GetAllPayments()
    {
        return _context.Payments
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToList();
    }

    public async Task<bool> ConfirmPaymentAsync(string orderId, string paymentId, string signature)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.RazorpayOrderId == orderId);

        if (payment == null) return false;

        // Verify Razorpay signature for security
        string keySecret = _configuration["Razorpay:KeySecret"] ?? "";
        string generatedSignature = "";
        
        using (var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(keySecret)))
        {
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(orderId + "|" + paymentId));
            generatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        if (generatedSignature != signature)
        {
            return false;
        }

        payment.RazorpayPaymentId = paymentId;
        payment.RazorpaySignature = signature;
        payment.Status = "Succeeded";
        payment.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ClearPendingPaymentsAsync()
    {
        var pending = await _context.Payments
            .Where(p => p.Status == "Pending" && !p.IsDeleted)
            .ToListAsync();
            
        _context.Payments.RemoveRange(pending);
        await _context.SaveChangesAsync();
    }
}
