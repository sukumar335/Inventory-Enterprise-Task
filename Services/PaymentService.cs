using Stripe;
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
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    public async Task<Core.Entities.Payment> CreatePaymentIntentAsync(decimal amount, string currency, string description)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), // Stripe works in cents/paise
            Currency = currency,
            Description = description,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);

        var payment = new Core.Entities.Payment
        {
            Description = description,
            Amount = amount,
            Currency = currency,
            Status = "Pending",
            StripePaymentIntentId = intent.Id,
            StripeClientSecret = intent.ClientSecret,
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

    public async Task<bool> ConfirmPaymentAsync(string paymentIntentId)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId);

        if (payment == null) return false;

        payment.Status = "Succeeded";
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ClearPendingPaymentsAsync()
    {
        var pending = await _context.Payments
            .Where(p => p.Status == "Pending" && !p.IsDeleted)
            .ToListAsync();
            
        // Physical delete for pending junk records
        _context.Payments.RemoveRange(pending);
        await _context.SaveChangesAsync();
    }
}
