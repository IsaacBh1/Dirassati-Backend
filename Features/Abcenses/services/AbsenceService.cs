using Dirassati_Backend.Common.Services.ConnectionTracker;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Abcenses.Dtos;
using Dirassati_Backend.Features.Absences.Repos;
using Dirassati_Backend.Features.Groups.Repos;
using Dirassati_Backend.Hubs;
using Dirassati_Backend.Hubs.HelperClasses;
using Microsoft.AspNetCore.SignalR;

namespace Dirassati_Backend.Features.Abcenses.services
{
    public class AbsenceService
    {
        private readonly IAbsenceRepository _absenceRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IHubContext<ParentNotificationHub> _hubContext;

        public AbsenceService(
            IAbsenceRepository absenceRepository,
            IGroupRepository groupRepository,
            IConnectionTracker connectionTracker,
            IHubContext<ParentNotificationHub> hubContext)
        {
            _absenceRepository = absenceRepository;
            _groupRepository = groupRepository;
            _hubContext = hubContext;
        }

        public async Task MarkAbsencesAsync(Guid groupId, List<Guid> absentStudentIds)
        {
            var group = await _groupRepository.GetGroupWithStudentsAsync(groupId);
            if (group == null) throw new ArgumentException("Group not found");

            var absences = group.Students
                .Where(s => absentStudentIds.Contains(s.StudentId))
                .Select(s => new Absence
                {
                    StudentId = s.StudentId,
                    DateTime = DateTime.Now,
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
                    .SendAsync("ReceiveAbsenceNotification", new AbsenceNotification
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

        public async Task<StudentAbsenceDto> GetStudentAbsencesAsync(Guid studentId, Guid parentId)
        {
            // Check if the student exists and retrieve their parent
            var student = await _groupRepository.GetStudentWithParentAsync(studentId);

            // Verify student exists and parent relationship
            if (student == null)
            {
                throw new ArgumentException("Student not found.");
            }

            if (student.ParentId != parentId)
            {
                throw new UnauthorizedAccessException("Parent is not related to the student.");
            }

            // Retrieve absences for the student
            var absences = await _absenceRepository.GetAbsencesByStudentIdAsync(studentId);

            // Map to DTO
            var studentAbsenceDto = new StudentAbsenceDto
            {
                StudentId = studentId,
                StudentName = $"{student.FirstName} {student.LastName}",
                Absences = absences.Select(a => new AbsenceDto
                {
                    AbsenceId = a.AbsenceId,
                    DateTime = a.DateTime,
                    IsJustified = a.IsJustified,
                    Remark = a.Remark ?? string.Empty,
                    IsNotified = a.IsNotified
                }).ToList()
            };

            return studentAbsenceDto;
        }
    }
}