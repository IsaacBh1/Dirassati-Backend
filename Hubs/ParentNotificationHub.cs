using System.Security.Claims;
using Dirassati_Backend.Features.Absences.Repos;
using Dirassati_Backend.Features.Parents.Repositories;
using DirassatiBackend.Common.Services.ConnectionTracker;
using Microsoft.AspNetCore.SignalR;

public class ParentNotificationHub : Hub
{
    private readonly IConnectionTracker _connectionTracker;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IParentRepository _parentRepository;
    public ParentNotificationHub(IConnectionTracker connectionTracker, IAbsenceRepository absenceRepository, IParentRepository parentRepository)
    {
        _connectionTracker = connectionTracker;
        _absenceRepository = absenceRepository;
        _parentRepository = parentRepository;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            _connectionTracker.AddConnection(userId, Context.ConnectionId);
        }
        await base.OnConnectedAsync();
        if (Guid.TryParse(userId, out var userGuid))
        {
            await SendPendingNotifications(userGuid);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            _connectionTracker.RemoveConnection(userId, Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    private async Task SendPendingNotifications(Guid userId)
    {
        var parent = await _parentRepository.GetParentByStudentIdAsync(userId);
        if (parent == null)
        {
            throw new InvalidOperationException("Parent not found.");
        }

        var students = await _parentRepository.GetStudentsByParentIdAsync(parent.ParentId);
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