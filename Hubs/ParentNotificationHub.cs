using AutoMapper;
using Dirassati_Backend.Common.Dtos;
using Dirassati_Backend.Common.Services.ConnectionTracker;
using Dirassati_Backend.Data.Enums;
using Dirassati_Backend.Features.Absences.Repos;
using Dirassati_Backend.Features.Parents.Dtos;
using Dirassati_Backend.Features.Parents.Repositories;
using Dirassati_Backend.Features.Payments.DTOs;
using Dirassati_Backend.Features.Teachers.Dtos;
using Dirassati_Backend.Hubs.HelperClasses;
using Dirassati_Backend.Hubs.Interfaces;
using Dirassati_Backend.Hubs.Services;
using Dirassati_Backend.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;


namespace Dirassati_Backend.Hubs;

[Authorize]
public class ParentNotificationHub(
    IConnectionTracker connectionTracker,
    IAbsenceRepository absenceRepository,
    IParentRepository parentRepository, IMapper mapper, AppDbContext dbContext, ILogger<ParentNotificationHub> logger, IParentNotificationServices parentNotificationServices) : Hub<IParentClient>
{
    private readonly IConnectionTracker _connectionTracker = connectionTracker;
    private readonly IAbsenceRepository _absenceRepository = absenceRepository;
    private readonly IParentRepository _parentRepository = parentRepository;
    private readonly IMapper _mapper = mapper;
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger<ParentNotificationHub> _logger = logger; // Add this
    private readonly IParentNotificationServices _parentNotificationServices = parentNotificationServices;

    public override async Task OnConnectedAsync()
    {
        var parentId = Context.User?.FindFirst("parentId")?.Value;
        if (!string.IsNullOrEmpty(parentId) && Guid.TryParse(parentId, out var parentGuid))
        {
            _logger.LogInformation("Parent {ParentId} connected with connection ID {ConnectionId}", parentId, Context.ConnectionId);

            _connectionTracker.AddConnection(parentId, Context.ConnectionId);

            await Groups.AddToGroupAsync(Context.ConnectionId, $"parent-{parentId}");
            _logger.LogInformation("Parent {ParentIdFirst} added to group parent-{ParentId}", parentId, parentId);

            var schoolIds = await _parentNotificationServices.GetSchoolIdsByParentIdAsync(parentGuid);

            foreach (var schoolId in schoolIds)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"parents-school-{schoolId}");
                _logger.LogInformation("Parent {ParentId} added to group parents-school-{SchoolId}", parentId, schoolId);
            }

            _logger.LogInformation("Sending pending notifications to parent {ParentId}", parentId);
            await SendPendingNotifications(parentGuid);
        }
        else
        {
            _logger.LogWarning("Failed to retrieve or parse parent ID from context for connection ID {ConnectionId}", Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }


    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var parentId = Context.User?.FindFirst("parentId")?.Value;
        if (!string.IsNullOrEmpty(parentId))
        {
            _connectionTracker.RemoveConnection(parentId, Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    private async Task SendPendingNotifications(Guid parentId)
    {
        var students = await _parentRepository.GetStudentsByParentIdAsync(parentId);
        var studentIds = students.Select(s => s.StudentId).ToList();
        var schoolIdsForParent = await _parentNotificationServices.GetSchoolIdsByParentIdAsync(parentId);
        await SendAbsenceNotifications(students, studentIds);
        await SendStudentReportNotifications(studentIds);
        await SendPendingBillAddedNotifications(schoolIdsForParent, parentId);
    }

    private async Task SendStudentReportNotifications(List<Guid> studentIds)
    {
        try
        {
            var reports = await _dbContext.StudentReports
                .Where(rep => studentIds.Contains(rep.StudentId))
                .Include(r => r.Student)

                .ToListAsync();

            foreach (var report in reports)
            {
                var teacher = await _dbContext.Teachers.FirstOrDefaultAsync(t => t.TeacherId == report.TeacherId);
                if (teacher == null)
                {
                    _logger.LogWarning("Cannot find teacher : {Id}", report.TeacherId);
                    return;

                }
                _dbContext.Entry(teacher).Reference(t => t.User).Load();
                var notification = _mapper.Map<GetStudentReportDto>(report);
                notification.Teacher = new BasePersonDto
                {
                    FirstName = teacher.User.FirstName,
                    LastName = teacher.User.FirstName,
                };

                await Clients.Caller.ReceiveNewReport(notification);
                if (report.StudentReportStatusId != (int)ReportStatusEnum.Sent)
                {
                    report.StudentReportStatusId = (int)ReportStatusEnum.Sent;
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending student report notifications");
        }
    }


    private async Task SendPendingBillAddedNotifications(List<Guid> schoolIds, Guid parentId)
    {
        try
        {

            var pendingBills = await _dbContext.Bills
                .Where(b => schoolIds.Contains(b.SchoolId))
                .ToListAsync();

            if (pendingBills.Count == 0)
            {
                _logger.LogInformation("No pending admin bills to notify for parent {ParentId}", parentId);
                return;
            }

            var notifications = pendingBills.Select(bill => new GetBillDto
            {
                Title = bill.Title,
                Description = bill.Description,
                Amount = bill.Amount,
            }).ToList();

            await Clients.Group($"parent-{parentId}").ReceivePendingBillNotification(notifications);



            _logger.LogInformation("Pending admin bill notifications sent for parent {ParentId}", parentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending pending admin bill notifications for school {ParentId}", parentId);
        }
    }

    private async Task SendAbsenceNotifications(IEnumerable<GetStudentDto> students, List<Guid> studentIds)
    {
        try
        {
            var pendingAbsences = await _absenceRepository.GetAbsencesByStudentIdsAsync(studentIds, false);

            foreach (var absence in pendingAbsences)
            {
                var student = students.FirstOrDefault(s => s.StudentId == absence.StudentId);
                if (student == null)
                {
                    _logger.LogWarning("Student with ID {Id} not found", absence.StudentId);
                    continue;
                }

                var notification = new AbsenceNotification
                {
                    StudentName = $"{student.FirstName} {student.LastName}",
                    Date = absence.DateTime
                };
                await Clients.Caller.ReceiveAbsenceNotification(notification);

                absence.IsNotified = true;
                await _absenceRepository.UpdateAbsenceAsync(absence);
            }

            await _absenceRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending absence notifications");
        }
    }





    public async Task BroadcastToAllClients(string message)
    {
        await Clients.All.ReceiveBroadcastNotification(message);
    }

}