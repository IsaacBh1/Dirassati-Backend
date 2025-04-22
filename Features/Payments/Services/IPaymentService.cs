using Dirassati_Backend.Common;
using Dirassati_Backend.Features.Payments.DTOs;

namespace Dirassati_Backend.Features.Payments.Services;


public interface IPaymentService
{
    // Takes billId, studentId, and the paying parent's ID
    Task<Result<InitiatePaymentResponseDto, string>> InitiateBillPaymentAsync(Guid studentId, Guid parentId);
    // Processes webhook data
    Task<Result<Unit, string>> HandleChargilyWebhookAsync(ChargilyWebhookPayload payload, string rawRequestBody, string signatureHeader);
}