using Dirassati_Backend.Features.Absences.Repos;
using Dirassati_Backend.Features.Parents.Repositories;
using DirassatiBackend.Common.Services.ConnectionTracker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[Authorize]
public class ParentNotificationHub : Hub
{
    private readonly IConnectionTracker _connectionTracker;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IParentRepository _parentRepository;

    public ParentNotificationHub(
        IConnectionTracker connectionTracker,
        IAbsenceRepository absenceRepository,
        IParentRepository parentRepository)
    {
        _connectionTracker = connectionTracker;
        _absenceRepository = absenceRepository;
        _parentRepository = parentRepository;
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
        var pendingAbsences = await _absenceRepository.GetAbsencesByStudentIdsAsync(studentIds, false);

        foreach (var absence in pendingAbsences)
        {
            var student = students.First(s => s.StudentId == absence.StudentId);
            await Clients.Caller.SendAsync("ReceiveAbsenceNotification",
                new
                {
                    StudentName = $"{student.FirstName} {student.LastName}",
                    Date = absence.DateTIme
                });

            absence.IsNotified = true;
            await _absenceRepository.UpdateAbsenceAsync(absence);
        }

        await _absenceRepository.SaveChangesAsync();
    }

    public async Task BroadcastToAllClients(string message)
    {
        await Clients.All.SendAsync("ReceiveBroadcastNotification", message);
    }
}