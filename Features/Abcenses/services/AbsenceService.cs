using Dirassati_Backend.Common.Services.ConnectionTracker;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Abcenses.Dtos;
using Dirassati_Backend.Features.Absences.Repos;
using Dirassati_Backend.Features.Groups.Repos;
using Dirassati_Backend.Hubs;
using Dirassati_Backend.Hubs.HelperClasses;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Dirassati_Backend.Features.Abcenses.services
{
    public class AbsenceService(
        IAbsenceRepository absenceRepository,
        IGroupRepository groupRepository,
        IHubContext<ParentNotificationHub> hubContext,
        ILogger<AbsenceService> logger)
    {
        private readonly IAbsenceRepository _absenceRepository = absenceRepository;
        private readonly IGroupRepository _groupRepository = groupRepository;
        private readonly IHubContext<ParentNotificationHub> _hubContext = hubContext;
        private readonly ILogger<AbsenceService> _logger = logger;

        public async Task MarkAbsencesAsync(Guid groupId, List<Guid> absentStudentIds)
        {
            _logger.LogInformation("Starting to mark absences for group: {GroupId}, Students count: {StudentCount}",
                groupId, absentStudentIds.Count);

            try
            {
                _logger.LogDebug("Fetching group with students for group ID: {GroupId}", groupId);
                var group = await _groupRepository.GetGroupWithStudentsAsync(groupId);

                if (group == null)
                {
                    _logger.LogWarning("Group not found with ID: {GroupId}", groupId);
                    throw new ArgumentException("Group not found");
                }

                _logger.LogDebug("Group found with {StudentCount} students", group.Students.Count);

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

                _logger.LogInformation("Created {AbsenceCount} absence records for group: {GroupId}",
                    absences.Count, groupId);

                foreach (var absence in absences)
                {
                    _logger.LogDebug("Adding absence record for student: {StudentId}", absence.StudentId);
                    await _absenceRepository.AddAbsenceAsync(absence);
                }

                _logger.LogDebug("Saving absence records to database");
                await _absenceRepository.SaveChangesAsync();

                _logger.LogInformation("Starting to notify parents for {AbsenceCount} absences", absences.Count);

                foreach (var absence in absences)
                {
                    var student = group.Students.First(s => s.StudentId == absence.StudentId);
                    var parentId = student.Parent.ParentId.ToString();

                    _logger.LogDebug("Sending absence notification to parent: {ParentId} for student: {StudentId}",
                        parentId, absence.StudentId);

                    await _hubContext.Clients.Group($"parent-{parentId}")
                        .SendAsync("ReceiveAbsenceNotification", new AbsenceNotification
                        {
                            StudentName = $"{student.FirstName} {student.LastName}",
                            Date = DateTime.Now
                        });

                    absence.IsNotified = true;
                    await _absenceRepository.UpdateAbsenceAsync(absence);
                }

                _logger.LogDebug("Saving notification status updates to database");
                await _absenceRepository.SaveChangesAsync();

                _logger.LogInformation("Successfully marked absences and notified parents for group: {GroupId}", groupId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while marking absences for group: {GroupId}", groupId);
                throw;
            }
        }

        public async Task TestBroadcastNotification()
        {
            _logger.LogInformation("Starting test broadcast notification to all clients");

            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveBroadcastNotification",
                    "📢 Test broadcast to all connected clients");

                _logger.LogInformation("Test broadcast notification sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending test broadcast notification");
                throw;
            }
        }

        public async Task<StudentAbsenceDto> GetStudentAbsencesAsync(Guid studentId, Guid parentId)
        {
            _logger.LogInformation("Retrieving absences for student: {StudentId} by parent: {ParentId}",
                studentId, parentId);

            try
            {
                _logger.LogDebug("Fetching student with parent information for student ID: {StudentId}", studentId);
                var student = await _groupRepository.GetStudentWithParentAsync(studentId);

                if (student == null)
                {
                    _logger.LogWarning("Student not found with ID: {StudentId}", studentId);
                    throw new ArgumentException("Student not found.");
                }

                if (student.ParentId != parentId)
                {
                    _logger.LogWarning("Unauthorized access attempt: Parent {ParentId} tried to access student {StudentId} data",
                        parentId, studentId);
                    throw new UnauthorizedAccessException("Parent is not related to the student.");
                }

                _logger.LogDebug("Parent-student relationship verified successfully");

                _logger.LogDebug("Retrieving absences for student: {StudentId}", studentId);
                var absences = await _absenceRepository.GetAbsencesByStudentIdAsync(studentId);

                _logger.LogInformation("Found {AbsenceCount} absences for student: {StudentId}",
                    absences.Count, studentId);

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

                _logger.LogInformation("Successfully retrieved absences for student: {StudentId}", studentId);
                return studentAbsenceDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving absences for student: {StudentId} by parent: {ParentId}",
                    studentId, parentId);
                throw;
            }
        }
    }
}