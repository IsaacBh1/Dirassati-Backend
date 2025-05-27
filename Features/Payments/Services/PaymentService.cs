using System.Net;
using System.Security.Cryptography;
using System.Text;
using Dirassati_Backend.Common;
using Dirassati_Backend.Data.Enums;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Payments.DTOs;
using Dirassati_Backend.Hubs;
using Dirassati_Backend.Hubs.Interfaces;
using Dirassati_Backend.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Parlot.Fluent;
namespace Dirassati_Backend.Features.Payments.Services;

public class PaymentService(
    AppDbContext context,
    IChargilyClient chargilyClient,
    IConfiguration configuration,
    ILogger<PaymentService> logger, IHubContext<ParentNotificationHub, IParentClient> hubContext) : IPaymentService
{
    private readonly AppDbContext _context = context;
    private readonly IChargilyClient _chargilyClient = chargilyClient;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<PaymentService> _logger = logger;
    private readonly IHubContext<ParentNotificationHub, IParentClient> _hubContext = hubContext;

    public async Task<Result<InitiatePaymentResponseDto, string>> InitiateBillPaymentAsync(Guid studentId, Guid parentId)
    {
        var result = new Result<InitiatePaymentResponseDto, string>();
        bool isFailed = false;
        //checking if there is a previous failed payment
        var bill = await _context.StudentPayments
            .Where(p => p.Status == PaymentStatus.Failed && p.StudentId == studentId)
            .Select(p => p.Bill)
            .FirstOrDefaultAsync();

        isFailed = bill is not null;
        var existingPayment = await _context.StudentPayments.AsNoTracking().Include(p => p.Bill).FirstOrDefaultAsync(p => p.StudentId == studentId && p.Status == PaymentStatus.Pending);
        bill ??= existingPayment?.Bill;

        if (bill == null) return result.Failure($"No bill active for student {studentId} was found.", (int)HttpStatusCode.NotFound);
        var billId = bill.BillId;

        // 2. Validate Student exists and belongs to the paying Parent
        // (Assuming Student has a ParentId property)
        var studentExists = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId && s.ParentId == parentId && s.IsActive);
        if (studentExists == null) return result.Failure($"Active Student with ID {studentId} not found for Parent ID {parentId}.", (int)HttpStatusCode.NotFound);

        // 3. Check if this specific bill has already been paid by this student
        var alreadyPaid = await _context.StudentPayments
            .AnyAsync(sp => sp.BillId == billId && sp.StudentId == studentId && sp.Status == PaymentStatus.Paid);
        if (alreadyPaid) return result.Failure("This bill has already been paid for this student.", (int)HttpStatusCode.BadRequest);

        _logger.LogInformation("Initiating payment for Bill {BillId}, Student {StudentId} by Parent {ParentId}", billId, studentId, parentId);

        // --- Prepare Payload ---
        var successUrl = _configuration["ChargilyConfigs:SuccessUrl"];
        var failureUrl = _configuration["ChargilyConfigs:FailureUrl"];
        var webhookUrl = _configuration["ChargilyConfigs:WebhookUrl"];
        // ... (check if URLs are configured) ...
        if (string.IsNullOrEmpty(successUrl))
            return result.Failure("Success URL is not configured.", (int)HttpStatusCode.InternalServerError);

        if (string.IsNullOrEmpty(failureUrl))
            return result.Failure("Failure URL is not configured.", (int)HttpStatusCode.InternalServerError);

        if (string.IsNullOrEmpty(webhookUrl))
            return result.Failure("Webhook URL is not configured.", (int)HttpStatusCode.InternalServerError);
        var checkoutPayload = new ChargilyCreateCheckoutRequest
        {
            Amount = (long)bill.Amount, // Assuming cents
            Currency = "dzd",
            success_url = successUrl!,
            failure_url = failureUrl!,
            webhook_endpoint = webhookUrl,

            Metadata = new Dictionary<string, string> {
                { "billId", billId.ToString() },
                { "studentId", studentId.ToString() },
                { "parentId", parentId.ToString() }
            }
        };

        try
        {
            var chargilyResponse = await _chargilyClient.CreateCheckoutSessionAsync(checkoutPayload);
            if (chargilyResponse == null) return result.Failure("Failed to create payment session.", (int)HttpStatusCode.InternalServerError);
            var responseDto = new InitiatePaymentResponseDto
            {
                CheckoutUrl = chargilyResponse.checkout_url,
                CheckoutId = chargilyResponse.id,


            };
            if (isFailed)
            {
                var failedPaymnent = await _context.StudentPayments.FirstOrDefaultAsync(p => p.StudentId == studentId && p.BillId == billId);
                if (failedPaymnent is null)
                    return result.Failure("Failed to fetch the failed payment.", (int)HttpStatusCode.InternalServerError);
                failedPaymnent.Status = PaymentStatus.Pending;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated failed payment record for Bill {BillId}, Student {StudentId} to Pending status.", billId, studentId);
                return result.Success(responseDto);
            }
            if (existingPayment is not null)
                existingPayment.PaymentGatewayCheckoutId = chargilyResponse.id;


            _logger.LogInformation("Webhook {EventId}: Creating new StudentPayment record for Bill {BillId}, Student {StudentId}.", chargilyResponse.id, billId, studentId);
            _logger.LogInformation("Chargily checkout session {CheckoutId} created for Bill {BillId}, Student {StudentId}. Redirect URL issued.", chargilyResponse.id, billId, studentId);


            await _context.SaveChangesAsync();
            return result.Success(responseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating Chargily payment for Bill {BillId}, Student {StudentId}.", billId, studentId);
            return result.Failure($"An unexpected error occurred: {ex.Message}", (int)HttpStatusCode.InternalServerError);
        }

    }


    public async Task<Result<Unit, string>> HandleChargilyWebhookAsync(ChargilyWebhookPayload payload, string rawRequestBody, string signatureHeader)
    {

        var result = new Result<Unit, string>();
        _logger.LogInformation("Webhook received. Event ID: {EventId}, Type: {EventType}, Checkout ID: {CheckoutId}", payload.Id, payload.Type, payload.Data?.Id);

        // 1. Verify Signature Manually
        var webhookSecret = _configuration["ChargilyConfigs:SecretKey"];
        if (!VerifyChargilySignature(webhookSecret!, rawRequestBody, signatureHeader))
        {
            _logger.LogWarning("Invalid Webhook Signature. Event ID: {EventId}", payload.Id);
            return result.Failure("Invalid signature.", (int)HttpStatusCode.Forbidden);
        }
        _logger.LogInformation("Webhook Signature Verified. Event ID: {EventId}", payload.Id);
        var paymentStatus = payload.Type?.ToLowerInvariant();
        // 2. Process based on event type
        switch (payload.Type?.ToLowerInvariant())
        {
            case "checkout.paid":
                // Extract required info from payload and metadata
                var checkoutId = payload.Data?.Id;
                var metadata = payload.Data?.Metadata;
                var amountPaid = payload.Data?.Amount ?? 0;

                if (string.IsNullOrEmpty(checkoutId) || metadata == null ||
                    !metadata.TryGetValue("billId", out var billIdStr) || !Guid.TryParse(billIdStr, out var billId) ||
                    !metadata.TryGetValue("studentId", out var studentIdStr) || !Guid.TryParse(studentIdStr, out var studentId) ||
                    !metadata.TryGetValue("parentId", out var parentIdStr) || !Guid.TryParse(parentIdStr, out var parentId))
                {
                    _logger.LogError("Webhook {EventId} ('checkout.paid') missing required data or metadata (billId, studentId, parentId).", payload.Id);
                    return result.Failure("Webhook payload missing required metadata.", (int)HttpStatusCode.BadRequest);
                }

                var existingPayment = await _context.StudentPayments
                    .FirstOrDefaultAsync(sp => sp.BillId == billId && sp.StudentId == studentId);
                if (existingPayment == null)
                {
                    _logger.LogError("Webhook {EventId}: No existing payment record found for Bill {BillId}, Student {StudentId} , Parent {Parent}.", payload.Id, billId, studentId, parentId);
                    return result.Failure("No existing payment record found.", (int)HttpStatusCode.NotFound);
                }
                if (existingPayment.Status == PaymentStatus.Paid)
                {
                    _logger.LogInformation("Webhook {EventId}: Payment for Bill {BillId}, Student {StudentId} already recorded as Paid. Ignoring duplicate.", payload.Id, billId, studentId);
                    return result.Success(Unit.Value); // Already processed successfully
                }


                if (existingPayment.Status == PaymentStatus.Pending)
                    _logger.LogInformation("Webhook {EventId}: Updating pending payment for Bill {BillId}, Student {StudentId} to Paid status.", payload.Id, billId, studentId);
                else
                    _logger.LogInformation("Webhook {EventId}: Updating previously failed payment for Bill {BillId}, Student {StudentId} to Paid.", payload.Id, billId, studentId);
                existingPayment.Status = PaymentStatus.Paid;
                existingPayment.UpdatedAt = DateTime.UtcNow;
                existingPayment.PaymentGatewayCheckoutId = checkoutId;
                existingPayment.PaymentGatewayTransactionId = payload.Data?.InvoiceId;
                existingPayment.AmountPaid = amountPaid;

                _context.StudentPayments.Update(existingPayment);



                await _context.SaveChangesAsync();
                await SendPaymentUpdateNotification(paymentStatus, billId, existingPayment);
                _logger.LogInformation("Webhook {EventId}: Successfully processed 'checkout.paid' for Bill {BillId}, Student {StudentId}.", payload.Id, billId, studentId);


                return result.Success(Unit.Value);

            case "checkout.failed":
                // Handle failed payment - Optional: Create a 'Failed' record
                var failedCheckoutId = payload.Data?.Id;
                var failedMetadata = payload.Data?.Metadata;
                // Extract IDs from metadata if present and needed for logging/tracking failed attempts
                if (failedMetadata != null &&
                    failedMetadata.TryGetValue("billId", out var failedBillIdStr) && Guid.TryParse(failedBillIdStr, out var failedBillId) &&
                    failedMetadata.TryGetValue("studentId", out var failedStudentIdStr) && Guid.TryParse(failedStudentIdStr, out var failedStudentId))
                {
                    _logger.LogWarning("Webhook {EventId}: Payment Failed for Bill {BillId}, Student {StudentId}. Checkout ID: {CheckoutId}",
                        payload.Id, failedBillId, failedStudentId, failedCheckoutId);


                    var existingFailed = await _context.StudentPayments.FirstOrDefaultAsync(sp => sp.BillId == failedBillId && sp.StudentId == failedStudentId);
                    if (existingFailed == null)
                    {
                        /* Create new record with Failed status */
                        var failedPayment = new StudentPayment
                        {
                            BillId = failedBillId,
                            StudentId = failedStudentId,
                            Status = PaymentStatus.Failed,
                            PaymentGatewayCheckoutId = failedCheckoutId,
                            PaymentGatewayTransactionId = payload.Data?.InvoiceId,
                            AmountPaid = payload.Data?.Amount ?? 0
                        };
                        _context.StudentPayments.Add(failedPayment);

                        _logger.LogInformation("Webhook {EventId}: Created failed payment record for Bill {BillId}, Student {StudentId}",
                            payload.Id, failedBillId, failedStudentId);
                        await SendPaymentUpdateNotification(paymentStatus, failedBillId, failedPayment);
                    }
                    else if (existingFailed.Status == PaymentStatus.Pending)
                    {
                        existingFailed.Status = PaymentStatus.Failed;
                        _context.StudentPayments.Update(existingFailed);
                        await SendPaymentUpdateNotification(paymentStatus, failedBillId, existingFailed);
                    }
                    await _context.SaveChangesAsync();

                }
                else
                {
                    _logger.LogWarning("Webhook {EventId}: Payment Failed. Checkout ID: {CheckoutId}. Metadata missing/incomplete.", payload.Id, failedCheckoutId);
                }

                return result.Success(Unit.Value); // Acknowledge webhook

            default:
                _logger.LogInformation("Webhook {EventId}: Unhandled event type: {EventType}", payload.Id, payload.Type);
                return result.Success(Unit.Value);
        }
    }

    // Manual Signature Verification Helper (Keep as is)
    private bool VerifyChargilySignature(string secret, string requestBodyJsonString, string signatureHeader)
    {
        if (string.IsNullOrEmpty(signatureHeader)) return false;
        try
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            using var hmac = new HMACSHA256(keyBytes);
            var bodyBytes = Encoding.UTF8.GetBytes(requestBodyJsonString);
            var computedHashBytes = hmac.ComputeHash(bodyBytes);
            var computedSignature = BitConverter.ToString(computedHashBytes).Replace("-", "").ToLowerInvariant();

            return computedSignature == signatureHeader;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during manual Chargily webhook signature verification.");
            return false;
        }
    }
    private async Task<bool> SendPaymentUpdateNotification(string? status, Guid billId, StudentPayment studentPayment)
    {
        var bill = await _context.Bills.Select(b => new { b.BillId, b.Title, b.Description, b.Amount }).FirstOrDefaultAsync(b => b.BillId == billId);
        if (bill == null)
        {
            _logger.LogWarning("Can't get the bill entitiy for {BillId}", billId);
            return false;
        }
        else if (status == null)
        {
            _logger.LogWarning("Payment status is null for Bill {BillId}", billId);
            return false;
        }

        var paymentNotification = new StudentPaymentBillDto
        {
            Amount = bill.Amount,
            BillId = bill.BillId,
            CreatedAt = studentPayment.CreatedAt,
            Description = bill.Description,
            Title = bill.Title,
            PaymentStatus = status,
        };
        await _hubContext.Clients.Group($"parent-{studentPayment.ParentId}").ReceivePaymentBillUpdate(paymentNotification);
        _logger.LogInformation("Sending update notification to {Group}", "parent-" + studentPayment.ParentId);
        return true;
    }
}