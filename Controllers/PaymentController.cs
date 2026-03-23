using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventoryEnterpriseProject.Core.Interfaces;
using Stripe;

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

    // GET: /Payment/Create — show checkout form
    public IActionResult Create()
    {
        ViewBag.PublishableKey = _configuration["Stripe:PublishableKey"];
        return View();
    }

    // POST: /Payment/Create — called by JS to create a PaymentIntent
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request)
    {
        if (request.Amount <= 0 || string.IsNullOrWhiteSpace(request.Description))
            return BadRequest(new { error = "Amount and description are required." });

        var payment = await _paymentService.CreatePaymentIntentAsync(
            request.Amount, request.Currency ?? "usd", request.Description);

        return Json(new
        {
            clientSecret = payment.StripeClientSecret,
            paymentId = payment.Id
        });
    }

    // GET: /Payment/Success
    public IActionResult Success()
    {
        return View();
    }

    // POST: /Payment/Webhook — Stripe sends signed events here
    [HttpPost]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var webhookSecret = _configuration["Stripe:WebhookSecret"];

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                webhookSecret
            );

            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent != null)
                    await _paymentService.ConfirmPaymentAsync(paymentIntent.Id);
            }

            return Ok();
        }
        catch (StripeException e)
        {
            return BadRequest(e.Message);
        }
    }
}

// Request model for the AJAX POST
public class CreatePaymentRequest
{
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? Description { get; set; }
}
