using Dirassati_Backend.Common;
using Dirassati_Backend.Features.Payments.DTOs;
using Dirassati_Backend.Features.Payments.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Buffers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Dirassati_Backend.Features.Payments;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger, BillServices billServices) : BaseController
{
    private readonly IPaymentService _paymentService = paymentService;
    // Cache JsonSerializerOptions as a static readonly field
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly ILogger<PaymentsController> _logger = logger;
    private readonly BillServices _billServices = billServices;

    [HttpPost("initiate")]

    public async Task<ActionResult<InitiatePaymentResponseDto>> InitiatePayment([FromBody] InitiatePaymentRequestDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var parentIdClaim = User.FindFirstValue("parentId"); // Assuming parentId is in JWT claims
        if (string.IsNullOrEmpty(parentIdClaim) || !Guid.TryParse(parentIdClaim, out var parentId))
        {
            return Unauthorized("Invalid or missing Parent ID claim.");
        }

        try
        {
            // Pass BillId, StudentId, and ParentId to the service
            var result = await _paymentService.InitiateBillPaymentAsync(request.StudentId, parentId);
            return HandleResult(result); // Use BaseController helper
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating payment for Student {StudentId}", request.StudentId);
            return StatusCode(500, "An internal error occurred during payment initiation.");
        }
    }

    // POST: api/payments/webhook/chargily
    [HttpPost("webhook/chargily")]
    [AllowAnonymous] // MUST be anonymous
    public async Task<IActionResult> ChargilyWebhook()
    {
        string rawRequestBody;
        try
        {
            var reader = Request.BodyReader;
            var result = await reader.ReadAsync();
            var buffer = result.Buffer;

            rawRequestBody = Encoding.UTF8.GetString(buffer.ToArray());
            reader.AdvanceTo(buffer.End); // Mark the buffer as consumed
            _logger.LogInformation("Recieved payload from webhook \n: {RawRequestBody}", rawRequestBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading request body from Chargily webhook: {ErrorMessage}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error reading request body.");
        }

        var signatureHeader = Request.Headers["signature"].FirstOrDefault();
        if (string.IsNullOrEmpty(signatureHeader))
        {

            _logger.LogWarning("Received webhook request without signature header");
            return BadRequest("Missing signature header.");
        }
        _logger.LogInformation("Recieved signature from webhook \n: {SignatureHeader}", signatureHeader);

        // Manually Deserialize
        ChargilyWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<ChargilyWebhookPayload>(rawRequestBody, _jsonSerializerOptions);
            if (payload == null || payload.Data == null)
            {


                _logger.LogWarning("Received webhook with null payload or data: {RawBody}", rawRequestBody);
                return BadRequest("Invalid payload format.");
            }
        }
        catch (JsonException jsonEx)
        {
            _logger.LogWarning(jsonEx, "Received invalid JSON webhook payload: {ErrorMessage}", jsonEx.Message);
            return BadRequest("Invalid JSON payload.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing webhook payload: {ErrorMessage}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deserializing payload.");
        }


        var paymentResult = await _paymentService.HandleChargilyWebhookAsync(payload!, rawRequestBody, signatureHeader);
        return HandleResult(paymentResult);

    }
    [HttpPost("add-bill")]
    [Authorize(Roles = "Employee")]
    public async Task<ActionResult<GetBillDto>> AddBill(AddBillDto request)
    {
        var schoolId = User.FindFirstValue("SchoolId");
        if (schoolId == null)
            return Unauthorized();
        try
        {
            var result = await _billServices.AddBillAsync(schoolId, request);

            return HandleResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding a new bill for School {SchoolId}", schoolId);
            return StatusCode(500, "An internal error occurred while adding the bill.");
        }
    }

    [HttpGet("student/{studentId}/bills")]
    [Authorize] // Ensure the user is authenticated
    public async Task<ActionResult<List<StudentPaymentBillDto>>> GetStudentPaymentBills(Guid studentId)
    {
        var parentId = User.FindFirstValue("parentId");
        var SchoolId = User.FindFirstValue("SchoolId");
        try
        {
            var result = await _billServices.GetStudentPaymentBillsAsync(studentId, SchoolId, parentId);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to fetch payment bills for student {StudentId}: {ErrorMessage}", studentId, result.Errors);

            }

            return HandleResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching payment bills for student {StudentId}", studentId);
            return StatusCode(500, "An internal error occurred while fetching payment bills.");
        }
    }
}