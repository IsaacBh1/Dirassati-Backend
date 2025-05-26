using System.Net;
using System.Security.Claims;
using AutoMapper;
using Dirassati_Backend.Common;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Payments.DTOs;
using Dirassati_Backend.Hubs;
using Dirassati_Backend.Hubs.Interfaces;
using Dirassati_Backend.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Features.Payments.Services;

public class BillServices(AppDbContext context, IMapper mapper, IHubContext<ParentNotificationHub, IParentClient> hubContext, IHttpContextAccessor httpContext, ILogger<BillServices> logger)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IHubContext<ParentNotificationHub, IParentClient> _hubContext = hubContext;
    private readonly IHttpContextAccessor _httpContext = httpContext;
    private readonly ILogger<BillServices> _logger = logger;

    public async Task<Result<GetBillDto, string>> AddBillAsync(string schoolId, AddBillDto billDto)
    {
        var result = new Result<GetBillDto, string>();
        if (!Guid.TryParse(schoolId, out Guid schoolIdGuid))
            return result.Failure("School Id Not Valid", 400);

        var school = await _context.Schools.Select(s => new { s.SchoolId }).FirstOrDefaultAsync(s => s.SchoolId == schoolIdGuid);
        if (school == null)
        {
            return result.Failure($"School with ID {schoolId} does not exist.", 404);
        }

        // Create a new bill
        var newBill = new Bill
        {
            SchoolId = schoolIdGuid,
            Amount = billDto.Amount,
            Title = billDto.Title,
            Description = billDto.Description,
            SchoolLevelId = billDto.SchoolLevelId
        };

        // Add the bill to the database
        try
        {
            _context.Bills.Add(newBill);
            await _context.SaveChangesAsync();
            var newBillResponse = _mapper.Map<GetBillDto>(newBill);
            await SendNewBillAddedNotification(newBillResponse);
            return result.Success(newBillResponse);
        }
        catch (Exception ex)
        {
            return result.Failure($"An error occurred while adding the bill: {ex.Message}", 500);
        }
    }
    public async Task<Result<List<StudentPaymentBillDto>, string>> GetStudentPaymentBillsAsync(Guid studentId, string? SchoolId, string? parentId)
    {
        var result = new Result<List<StudentPaymentBillDto>, string>();
        var verificationResult = await VerifyStudentAccessAsync(studentId, parentId, SchoolId);
        if (!verificationResult.IsSuccess)
        {
            return result.Failure(verificationResult.Errors!, verificationResult.StatusCode);
        }
        // Fetch the student's payment bills
        var payments = await _context.StudentPayments
            .Include(sp => sp.Bill) // Include related Bill data
            .Where(sp => sp.StudentId == studentId)
            .ToListAsync();

        if (payments.Count == 0)
        {
            return result.Failure($"No payment bills found for student with ID {studentId}.", (int)HttpStatusCode.NotFound);
        }

        var paymentDtos = payments.Select(sp => new StudentPaymentBillDto
        {
            BillId = sp.BillId,
            Title = sp.Bill.Title,
            Description = sp.Bill.Description,
            Amount = sp.Bill.Amount,
            PaymentStatus = sp.Status.ToString(),
            CreatedAt = sp.CreatedAt
        }).ToList();

        return result.Success(paymentDtos);
    }
    private async Task<Result<bool, string>> VerifyStudentAccessAsync(Guid studentId, string? parentId, string? schoolId)
    {
        var result = new Result<bool, string>();
        if (!string.IsNullOrEmpty(parentId))
        {
            if (!Guid.TryParse(parentId, out Guid parentIdGuid))
            {
                return result.Failure("Invalid Parent ID format.", (int)HttpStatusCode.BadRequest);
            }

            var studentExistsForParent = await _context.Students
                .AnyAsync(s => s.StudentId == studentId && s.ParentId == parentIdGuid);

            if (!studentExistsForParent)
            {
                return result.Failure("The specified parent does not have access to this student's bills.", (int)HttpStatusCode.Forbidden);
            }
        }
        else if (!string.IsNullOrEmpty(schoolId))
        {
            if (!Guid.TryParse(schoolId, out Guid schoolIdGuid))
            {
                return result.Failure("Invalid School ID format.", (int)HttpStatusCode.BadRequest);
            }

            var studentExistsForSchool = await _context.Students
                .AnyAsync(s => s.StudentId == studentId && s.SchoolId == schoolIdGuid);

            if (!studentExistsForSchool)
            {
                return result.Failure("The specified student does not belong to this school.", (int)HttpStatusCode.Forbidden);
            }
        }
        else
        {
            return result.Failure("Either Parent ID or School ID must be provided.", (int)HttpStatusCode.BadRequest);
        }

        return result.Success(true);
    }


    private async Task SendNewBillAddedNotification(GetBillDto billDto)
    {
        var schoolId = _httpContext.HttpContext?.User.FindFirstValue("SchoolId");
        if (schoolId == null) return;
        await _hubContext.Clients.Group($"parents-school-{schoolId}").ReceiveNewStudentBill(billDto);
        _logger.LogInformation("New bill notification sent for group {Group} : , Title: {Title}, Amount: {Amount}",
        "parents-school-" + schoolId,
             billDto.Title, billDto.Amount);
    }
}
