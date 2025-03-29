using Dirassati_Backend.Features.Absences.Repos;
using Dirassati_Backend.Features.Parents.Repositories;
using Dirassati_Backend.Common.Services.ConnectionTracker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Dirassati_Backend.Hubs.Interfaces;
using Dirassati_Backend.Hubs.HelperClasses;
using AutoMapper;
using Dirassati_Backend.Features.Teachers.Dtos;
using Dirassati_Backend.Features.Parents.Dtos;
using Persistence;
using Microsoft.EntityFrameworkCore;
using Dirassati_Backend.Data.Enums;
using Dirassati_Backend.Common.Dtos; // Add this

[Authorize]
public class ParentNotificationHub : Hub<IParentClient>
{
    private readonly IConnectionTracker _connectionTracker;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IParentRepository _parentRepository;
    private readonly IMapper _mapper;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ParentNotificationHub> _logger; // Add this

    public ParentNotificationHub(
        IConnectionTracker connectionTracker,
        IAbsenceRepository absenceRepository,
        IParentRepository parentRepository, IMapper mapper, AppDbContext dbContext, ILogger<ParentNotificationHub> logger) // Add this
    {
        _connectionTracker = connectionTracker;
        _absenceRepository = absenceRepository;
        _parentRepository = parentRepository;
        _mapper = mapper;
        _dbContext = dbContext;
        _logger = logger; // Add this
    }

    public override async Task OnConnectedAsync()
    {
        var parentId = Context.User?.FindFirst("parentId")?.Value;
        if (!string.IsNullOrEmpty(parentId))
        {
            // Track by parentId instead of user ID
            _connectionTracker.AddConnection(parentId, Context.ConnectionId);

            // Add connection to parent-specific group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"parent-{parentId}");

            if (Guid.TryParse(parentId, out var parentGuid))
            {
                await SendPendingNotifications(parentGuid);
            }
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
        await SendAbsenceNotifications(students, studentIds);
        await SendStudentReportNotifications(studentIds, students);
    }

    private async Task SendStudentReportNotifications(List<Guid> studentIds, IEnumerable<getStudentDto> students)
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
                    _logger.LogWarning($"Cannot find teacher : {report.TeacherId}");
                    return;

                }
                _dbContext.Entry(teacher).Reference(t => t.User).Load();
                var notification = _mapper.Map<GetStudentReportDto>(report);
                notification.Teacher = new SimpleTeacherDto
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

    private async Task SendAbsenceNotifications(IEnumerable<getStudentDto> students, List<Guid> studentIds)
    {
        try
        {
            var pendingAbsences = await _absenceRepository.GetAbsencesByStudentIdsAsync(studentIds, false);

            foreach (var absence in pendingAbsences)
            {
                var student = students.FirstOrDefault(s => s.StudentId == absence.StudentId);
                if (student == null)
                {
                    _logger.LogWarning($"Student with ID {absence.StudentId} not found");
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