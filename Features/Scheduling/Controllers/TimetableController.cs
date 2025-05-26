using System.ComponentModel.DataAnnotations;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Scheduling.Dtos;
using Dirassati_Backend.Features.Scheduling.Services;
using Dirassati_Backend.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Features.Scheduling;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly AdvancedScheduler _schedulingService;
    private readonly AppDbContext _context;

    public ScheduleController(AdvancedScheduler schedulingService, AppDbContext context)
    {
        _schedulingService = schedulingService;
        _context = context;
    }



    [HttpPost("schools/{schoolId}/configure")]
    public async Task<IActionResult> ConfigureSchoolSchedule(
     [FromRoute, Required] Guid schoolId,
     [FromBody] SchoolConfigRequest config)
    {
        try
        {
            var school = await _context.Schools
                .Include(s => s.ScheduleConfig)
                .FirstOrDefaultAsync(s => s.SchoolId == schoolId);

            if (school == null)
                return NotFound(new { Error = $"School with ID {schoolId} not found" });

            // Update or create configuration
            school.ScheduleConfig = new SchoolScheduleConfig
            {
                SchoolId = schoolId, // Ensure SchoolId is set
                MorningStart = config.MorningStart,
                MorningEnd = config.MorningEnd,
                AfternoonStart = config.AfternoonStart,
                AfternoonEnd = config.AfternoonEnd,
                DaysOff = config.DaysOff,
                ShortDays = config.ShortDays
                // Note: FullDays is not set as it's a read-only property
            };

            await _context.SaveChangesAsync();

            // Map to DTO
            var responseDto = new ScheduleConfigResponseDto
            {
                ConfigId = school.ScheduleConfig.ConfigId,
                SchoolId = school.ScheduleConfig.SchoolId,
                MorningStart = school.ScheduleConfig.MorningStart,
                MorningEnd = school.ScheduleConfig.MorningEnd,
                AfternoonStart = school.ScheduleConfig.AfternoonStart,
                AfternoonEnd = school.ScheduleConfig.AfternoonEnd,
                FullDays = school.ScheduleConfig.FullDays,
                ShortDays = school.ScheduleConfig.ShortDays,
                DaysOff = school.ScheduleConfig.DaysOff
            };

            return Ok(new { Success = true, ScheduleConfig = responseDto });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Configuration failed", Details = ex.Message });
        }
    }




    [HttpPost("schools/{schoolId}/generate/{academicYearId}")]
    public async Task<IActionResult> GenerateSchedule(
        [FromRoute, Required] Guid schoolId,
        [FromRoute, Range(2020, 2100)] int academicYearId)
    {
        try
        {
            // Validate school existence
            var school = await _context.Schools
                .Include(s => s.AcademicYear)
                .FirstOrDefaultAsync(s => s.SchoolId == schoolId);

            if (school == null)
                return NotFound(new { Error = $"School with ID {schoolId} not found" });

            // Validate academic year
            if (school.AcademicYearId != academicYearId)
                return BadRequest(new { Error = "Academic year mismatch for school" });

            // Check for existing schedule
            if (await _context.Lessons.AnyAsync(l =>
                l.SchoolId == schoolId && l.AcademicYearId == academicYearId))
            {
                return Conflict(new
                {
                    Error = "Schedule already exists for this academic year",
                    Action = "Use update endpoint or clear existing schedule first"
                });
            }

            // Generate new schedule
            var result = _schedulingService.GenerateSchedule(schoolId, academicYearId);

            // Save to database
            await _context.Lessons.AddRangeAsync(result.TeacherSchedules);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                TotalLessons = result.TeacherSchedules.Count,
                ConflictsResolved = result.TotalConflicts,
                Schedule = FormatSchedule(result.TeacherSchedules)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = "Schedule generation failed",
                Details = ex.Message
            });
        }
    }

    [HttpGet("schools/{schoolId}/schedule/{academicYearId}")]
    public async Task<IActionResult> GetSchedule(
        [FromRoute, Required] Guid schoolId,
        [FromRoute, Range(2020, 2100)] int academicYearId)
    {
        try
        {
            var schedule = await _context.Lessons
                .Include(l => l.Teacher)
                .Include(l => l.Classroom)
                .Include(l => l.Subject)
                .Include(l => l.Group)
                .Where(l => l.SchoolId == schoolId && l.AcademicYearId == academicYearId)
                .ToListAsync();

            if (!schedule.Any())
                return NotFound(new { Error = "No schedule found for given parameters" });

            return Ok(new
            {
                Success = true,
                TotalLessons = schedule.Count,
                Schedule = FormatSchedule(schedule)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = "Failed to retrieve schedule",
                Details = ex.Message
            });
        }
    }

    [HttpPost("teachers/{teacherId}/availability")]
    public async Task<IActionResult> SetTeacherAvailability(
        [FromRoute, Required] Guid teacherId,
        [FromBody] List<AvailabilityRequest> availability)
    {
        try
        {
            var teacher = await _context.Teachers
                .Include(t => t.Availabilities)
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);

            if (teacher == null)
                return NotFound(new { Error = $"Teacher with ID {teacherId} not found" });

            // Convert request to entities
            var newAvailability = availability.Select(a => new TeacherAvailability
            {
                TeacherId = teacherId,
                Day = a.Day,
                StartTime = a.StartTime,
                EndTime = a.EndTime
            }).ToList();

            // Update availability
            teacher.Availabilities = newAvailability.Cast<TeacherAvailability?>().ToList();
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                UpdatedEntries = newAvailability.Count,
                TeacherId = teacherId
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = "Failed to update teacher availability",
                Details = ex.Message
            });
        }
    }

    private static object FormatSchedule(List<Lesson> lessons)
    {
        return lessons
            .GroupBy(l => l.Timeslot?.Day)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Day = g.Key?.ToString() ?? "Unknown",
                Lessons = g.OrderBy(l => l.Timeslot?.StartTime)
                           .Select(l => new
                           {
                               Subject = l?.Subject?.Name ?? "Unknown",
                               Teacher = l?.Teacher?.User != null
                                   ? l.Teacher.User.FirstName + " " + l.Teacher.User.LastName
                                   : "Unknown",
                               TeacherId = l?.Teacher?.TeacherId ?? Guid.Empty,
                               Classroom = l?.Classroom?.ClassName ?? "Unknown",
                               Time = l?.Timeslot != null
                                   ? $"{l.Timeslot.StartTime:hh\\:mm} - {l.Timeslot.EndTime:hh\\:mm}"
                                   : "Unknown",
                               Group = l?.Group?.GroupId
                           })
            });
    }


    [HttpPost("schools/{schoolId}/subjects/hours")]
    public async Task<IActionResult> SetSubjectHours(
    [FromRoute, Required] Guid schoolId,
    [FromBody] List<SubjectHoursRequest> requests)
    {
        try
        {
            var existing = await _context.LevelSubjectHours
                .Where(lsh => lsh.SchoolId == schoolId)
                .ToListAsync();

            // Remove old entries
            _context.LevelSubjectHours.RemoveRange(existing);

            // Add new entries
            var newEntries = requests.Select(r => new LevelSubjectHours
            {
                SchoolId = schoolId,
                LevelId = r.LevelId,
                SubjectId = r.SubjectId,
                HoursPerWeek = r.Hours,
                Priority = r.Priority
            }).ToList();

            await _context.LevelSubjectHours.AddRangeAsync(newEntries);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                UpdatedEntries = newEntries.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = "Failed to update subject hours",
                Details = ex.Message
            });
        }
    }

    [HttpGet("schools/{schoolId}/subjects/hours")]
    public async Task<IActionResult> GetSubjectHours(
        [FromRoute, Required] Guid schoolId)
    {
        try
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var hours = await _context.LevelSubjectHours
                .Include(lsh => lsh.Subject)
                .Include(lsh => lsh.SchoolLevel)
                .Where(lsh => lsh.SchoolId == schoolId)
                .Select(lsh => new
                {
                    lsh.LevelId,
                    LevelName = lsh.SchoolLevel.LevelYear.ToString(),
                    lsh.SubjectId,
                    SubjectName = lsh.Subject.Name,
                    lsh.HoursPerWeek,
                    lsh.Priority
                })
                .ToListAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            return Ok(new
            {
                Success = true,
                SubjectHours = hours
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = "Failed to retrieve subject hours",
                Details = ex.Message
            });
        }
    }

    public class AvailabilityRequest
    {
        [Required]
        public DayOfWeek Day { get; set; }
        [Required]
        public TimeSpan StartTime { get; set; }
        [Required]
        public TimeSpan EndTime { get; set; }
    }
    public class SubjectHoursRequest
    {
        [Required] public int LevelId { get; set; }
        [Required] public int SubjectId { get; set; }
        [Range(1, 20)] public int Hours { get; set; }
        [Range(1, 10)] public int Priority { get; set; }
    }
}