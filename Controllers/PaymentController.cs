using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryEnterpriseProject.Core.Interfaces;

namespace InventoryEnterpriseProject.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _configuration;

    public PaymentController(IPaymentService paymentService, IConfiguration configuration)
    {
        _paymentService = paymentService;
        _configuration = configuration;
    }

    // GET: /Payment — list all payment records
    public IActionResult Index()
    {
        var payments = _paymentService.GetAllPayments();
        return View(payments);
    }
    
    // POST: /Payment/ClearPending
    [HttpPost]
    public async Task<IActionResult> ClearPending()
    {
        await _paymentService.ClearPendingPaymentsAsync();
        return RedirectToAction("Index");
    }

    // GET: /Payment/Create — show checkout form
    public IActionResult Create()
    {
        ViewBag.KeyId = _configuration["Razorpay:KeyId"];
        return View();
    }

    // POST: /Payment/Create — called by JS to create a Razorpay Order
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request)
    {
        if (request.Amount <= 0 || string.IsNullOrWhiteSpace(request.Description))
            return BadRequest(new { error = "Amount and description are required." });

        // Default currency to INR as Razorpay usually requires INR (though they support others)
        var currency = string.IsNullOrEmpty(request.Currency) ? "INR" : request.Currency.ToUpper();

        var payment = await _paymentService.CreateRazorpayOrderAsync(
            request.Amount, currency, request.Description);

        return Json(new
        {
            orderId = payment.RazorpayOrderId,
            paymentId = payment.Id
        });
    }

    // POST: /Payment/Confirm — callback when Razorpay is successful
    [HttpPost]
    public async Task<IActionResult> Confirm([FromBody] ConfirmPaymentRequest request)
    {
        if (string.IsNullOrEmpty(request.RazorpayOrderId) || string.IsNullOrEmpty(request.RazorpayPaymentId))
            return BadRequest();
            
        var success = await _paymentService.ConfirmPaymentAsync(
            request.RazorpayOrderId, 
            request.RazorpayPaymentId, 
            request.RazorpaySignature ?? "");
            
        return success ? Ok() : NotFound();
    }
}

// Request models
public class CreatePaymentRequest
{
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? Description { get; set; }
}

public class ConfirmPaymentRequest
{
    public string? RazorpayOrderId { get; set; }
    public string? RazorpayPaymentId { get; set; }
    public string? RazorpaySignature { get; set; }
}
