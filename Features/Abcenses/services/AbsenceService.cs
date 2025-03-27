using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Absences.Repos;
using Dirassati_Backend.Features.Groups.Repos;
using DirassatiBackend.Common.Services.ConnectionTracker;
using Microsoft.AspNetCore.SignalR;

namespace Dirassati_Backend.Features.Absences.Services
{
    public class AbsenceService
    {
        private readonly IAbsenceRepository _absenceRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IConnectionTracker _connectionTracker;
        private readonly IHubContext<ParentNotificationHub> _hubContext;

        public AbsenceService(
            IAbsenceRepository absenceRepository,
            IGroupRepository groupRepository,
            IConnectionTracker connectionTracker,
            IHubContext<ParentNotificationHub> hubContext)
        {
            _absenceRepository = absenceRepository;
            _groupRepository = groupRepository;
            _connectionTracker = connectionTracker;
            _hubContext = hubContext;
        }

        public async Task MarkAbsencesAsync(int groupId, List<Guid> absentStudentIds)
        {
            var group = await _groupRepository.GetGroupWithStudentsAsync(groupId);
            if (group == null) throw new ArgumentException("Group not found");

            var absences = group.Students
                .Where(s => absentStudentIds.Contains(s.StudentId))
                .Select(s => new Absence
                {
                    StudentId = s.StudentId,
                    DateTIme = DateTime.Now,
                    IsJustified = false,
                    Remark = "Absent",
                    IsNotified = false
                }).ToList();

            foreach (var absence in absences)
            {
                await _absenceRepository.AddAbsenceAsync(absence);
            }

            await _absenceRepository.SaveChangesAsync();

            foreach (var absence in absences)
            {
                var student = group.Students.First(s => s.StudentId == absence.StudentId);
                var parentId = student.Parent.ParentId.ToString();

                await _hubContext.Clients.Group($"parent-{parentId}")
                    .SendAsync("ReceiveAbsenceNotification", new
                    {
                        StudentName = $"{student.FirstName} {student.LastName}",
                        Date = DateTime.Now
                    });

                absence.IsNotified = true;
                await _absenceRepository.UpdateAbsenceAsync(absence);
            }

            await _absenceRepository.SaveChangesAsync();
        }
        public async Task TestBroadcastNotification()
        {
            await _hubContext.Clients.All.SendAsync("ReceiveBroadcastNotification",
                "📢 Test broadcast to all connected clients");
        }
    }
}