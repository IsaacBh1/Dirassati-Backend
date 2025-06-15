#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

using System.ComponentModel.DataAnnotations;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Scheduling.Dtos;
using Dirassati_Backend.Features.Scheduling.Services;
using Dirassati_Backend.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Features.Scheduling.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController(AdvancedScheduler schedulingService, AppDbContext context) : ControllerBase
{
    private readonly AdvancedScheduler _schedulingService = schedulingService;
    private readonly AppDbContext _context = context;

    [HttpPost("schools/{schoolId}/configure")]
    public async Task<IActionResult> ConfigureSchoolSchedule(
     [FromRoute, Required] Guid schoolId,
     [FromBody] SchoolConfigRequest config)
    {
        try
        {
            var school = await _context.Schools
                .Include(s => s.ScheduleConfig)
                .Include(s => s.Teachers)
                    .ThenInclude(t => t.Availabilities)
                .FirstOrDefaultAsync(s => s.SchoolId == schoolId);

            if (school == null)
                return NotFound(new { Error = $"School with ID {schoolId} not found" });

            school.ScheduleConfig = new SchoolScheduleConfig
            {
                SchoolId = schoolId,
                MorningStart = config.MorningStart,
                MorningEnd = config.MorningEnd,
                AfternoonStart = config.AfternoonStart,
                AfternoonEnd = config.AfternoonEnd,
                DaysOff = config.DaysOff,
                ShortDays = config.ShortDays,
                FullDays = config.FullDays
            };

            // Generate default availability for all teachers in this school
            await GenerateDefaultTeacherAvailability(school);

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

            return Ok(new
            {
                Success = true,
                ScheduleConfig = responseDto,
                Message = "Schedule configured and default teacher availability generated"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Configuration failed", Details = ex.Message });
        }
    }

    /// <summary>
    /// Generates default availability for all teachers based on school schedule configuration
    /// </summary>
    private async Task GenerateDefaultTeacherAvailability(Data.Models.School school)
    {
        if (school.ScheduleConfig == null || school.Teachers == null)
            return;

        var config = school.ScheduleConfig;

        foreach (var teacher in school.Teachers)
        {
            // Clear existing availability
            if (teacher.Availabilities != null)
            {
                _context.TeacherAvailabilities.RemoveRange(teacher.Availabilities.Where(a => a != null)!);
            }

            var newAvailabilities = new List<TeacherAvailability>();

            // Generate availability for each day of the week
            foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
            {
                // Skip days off
                if (config.DaysOff.Contains(day))
                    continue;

                if (config.FullDays.Contains(day))
                {
                    // Full day: morning and afternoon sessions
                    newAvailabilities.Add(new TeacherAvailability
                    {
                        TeacherId = teacher.TeacherId,
                        Day = day,
                        StartTime = config.MorningStart,
                        EndTime = config.MorningEnd
                    });

                    newAvailabilities.Add(new TeacherAvailability
                    {
                        TeacherId = teacher.TeacherId,
                        Day = day,
                        StartTime = config.AfternoonStart,
                        EndTime = config.AfternoonEnd
                    });
                }
                else if (config.ShortDays.Contains(day))
                {
                    // Short day: morning session only
                    newAvailabilities.Add(new TeacherAvailability
                    {
                        TeacherId = teacher.TeacherId,
                        Day = day,
                        StartTime = config.MorningStart,
                        EndTime = config.MorningEnd
                    });
                }
            }

            await _context.TeacherAvailabilities.AddRangeAsync(newAvailabilities);
        }
    }

    /// <summary>
    /// Sets default availability for a specific teacher based on school configuration
    /// </summary>
    [HttpPost("teachers/{teacherId}/generate-default-availability")]
    public async Task<IActionResult> GenerateDefaultAvailabilityForTeacher(
        [FromRoute, Required] Guid teacherId)
    {
        try
        {
            var teacher = await _context.Teachers
                .Include(t => t.Availabilities)
                .Include(t => t.School)
                    .ThenInclude(s => s.ScheduleConfig)
                .FirstOrDefaultAsync(t => t.TeacherId == teacherId);

            if (teacher == null)
                return NotFound(new { Error = $"Teacher with ID {teacherId} not found" });

            if (teacher.School?.ScheduleConfig == null)
                return BadRequest(new { Error = "School schedule configuration not found" });

            // Clear existing availability
            if (teacher.Availabilities != null)
            {
                _context.TeacherAvailabilities.RemoveRange(teacher.Availabilities.Where(a => a != null)!);
            }

            var config = teacher.School.ScheduleConfig;
            var newAvailabilities = new List<TeacherAvailability>();

            // Generate availability for each day of the week
            foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
            {
                // Skip days off
                if (config.DaysOff.Contains(day))
                    continue;

                if (config.FullDays.Contains(day))
                {
                    // Full day: morning and afternoon sessions
                    newAvailabilities.Add(new TeacherAvailability
                    {
                        TeacherId = teacherId,
                        Day = day,
                        StartTime = config.MorningStart,
                        EndTime = config.MorningEnd
                    });

                    newAvailabilities.Add(new TeacherAvailability
                    {
                        TeacherId = teacherId,
                        Day = day,
                        StartTime = config.AfternoonStart,
                        EndTime = config.AfternoonEnd
                    });
                }
                else if (config.ShortDays.Contains(day))
                {
                    // Short day: morning session only
                    newAvailabilities.Add(new TeacherAvailability
                    {
                        TeacherId = teacherId,
                        Day = day,
                        StartTime = config.MorningStart,
                        EndTime = config.MorningEnd
                    });
                }
            }

            await _context.TeacherAvailabilities.AddRangeAsync(newAvailabilities);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Message = "Default availability generated successfully",
                TeacherId = teacherId,
                AvailabilitySlots = newAvailabilities.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = "Failed to generate default availability",
                Details = ex.Message
            });
        }
    }

    /// <summary>
    /// Regenerates default availability for all teachers in a school
    /// </summary>
    [HttpPost("schools/{schoolId}/regenerate-teacher-availability")]
    public async Task<IActionResult> RegenerateAllTeacherAvailability(
        [FromRoute, Required] Guid schoolId)
    {
        try
        {
            var school = await _context.Schools
                .Include(s => s.ScheduleConfig)
                .Include(s => s.Teachers)
                    .ThenInclude(t => t.Availabilities)
                .FirstOrDefaultAsync(s => s.SchoolId == schoolId);

            if (school == null)
                return NotFound(new { Error = $"School with ID {schoolId} not found" });

            if (school.ScheduleConfig == null)
                return BadRequest(new { Error = "School schedule configuration not found" });

            await GenerateDefaultTeacherAvailability(school);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Message = "Default availability regenerated for all teachers",
                TeachersUpdated = school.Teachers?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Error = "Failed to regenerate teacher availability",
                Details = ex.Message
            });
        }
    }

    [HttpPost("schools/{schoolId}/generate/{academicYearId}")]
    public async Task<IActionResult> GenerateSchedule(
        [FromRoute, Required] Guid schoolId,
        [FromRoute, Range(0, 2100)] int academicYearId)
    {
        try
        {
            // Validate school existence
            var school = await _context.Schools
                .Include(s => s.AcademicYear)
                .FirstOrDefaultAsync(s => s.SchoolId == schoolId);

            if (school == null)
                return NotFound(new { Error = $"School with ID {schoolId} not found" });

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
            Console.WriteLine($"Starting schedule generation for school {schoolId}");

            var result = _schedulingService.GenerateSchedule(schoolId, academicYearId);

            Console.WriteLine($"Generated {result.TeacherSchedules.Count} lessons");

            if (result.TeacherSchedules.Count == 0)
            {
                return Ok(new
                {
                    Success = false,
                    Message = "No lessons were generated. Check data requirements.",
                    TotalLessons = 0,
                    ConflictsResolved = result.TotalConflicts,
                    result.HoursCompliance
                });
            }

            await _context.Lessons.AddRangeAsync(result.TeacherSchedules);
            var savedCount = await _context.SaveChangesAsync();

            Console.WriteLine($"Saved {savedCount} lessons to database");

            return Ok(new
            {
                Success = true,
                TotalLessons = result.TeacherSchedules.Count,
                ConflictsResolved = result.TotalConflicts,
                SavedLessons = savedCount,
                Schedule = FormatSchedule(result.TeacherSchedules)
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new
            {
                Error = "Schedule generation failed",
                Details = ex.Message
            });
        }
    }

    [HttpGet("schools/{schoolId}/validate-data")]
    public async Task<IActionResult> ValidateSchedulingData([FromRoute, Required] Guid schoolId)
    {
        var school = await _context.Schools
            .Include(s => s.ScheduleConfig)
            .Include(s => s.Groups)
                .ThenInclude(g => g.Classroom)
                    .ThenInclude(c => c!.SchoolLevel)
            .Include(s => s.Teachers)
                .ThenInclude(t => t.Availabilities)
            .Include(s => s.Teachers)
                .ThenInclude(t => t.Subjects)
            .Include(s => s.Classrooms)
            .Include(s => s.Timeslots)
            .FirstOrDefaultAsync(s => s.SchoolId == schoolId);

        if (school == null)
            return NotFound();

        var levelSubjectHours = await _context.LevelSubjectHours
            .Include(lsh => lsh.Subject)
            .Include(lsh => lsh.SchoolLevel)
            .Where(lsh => lsh.SchoolId == schoolId)
            .ToListAsync();

        var detailedValidation = new
        {
            SchoolExists = school != null,
            HasScheduleConfig = school!.ScheduleConfig != null,
            ScheduleConfig = school.ScheduleConfig != null ? new
            {
                school.ScheduleConfig.MorningStart,
                school.ScheduleConfig.MorningEnd,
                school.ScheduleConfig.AfternoonStart,
                school.ScheduleConfig.AfternoonEnd,
                FullDays = school.ScheduleConfig.FullDays?.Length ?? 0,
                ShortDays = school.ScheduleConfig.ShortDays?.Length ?? 0,
                DaysOff = school.ScheduleConfig.DaysOff?.Length ?? 0
            } : null,

            Groups = new
            {
                Total = school.Groups?.Count ?? 0,
                WithClassrooms = school.Groups?.Count(g => g.Classroom != null) ?? 0,
                WithSchoolLevel = school.Groups?.Count(g => g.Classroom?.SchoolLevel != null) ?? 0,
                Details = school.Groups?.Select(g => new
                {
                    g.GroupId,
                    g.GroupName,
                    HasClassroom = g.Classroom != null,
                    g.ClassroomId,
                    g.Classroom?.SchoolLevelId,
                    g.Classroom?.SchoolLevel?.LevelYear
                }).ToList()
            },

            Teachers = new
            {
                Total = school.Teachers?.Count ?? 0,
                WithAvailability = school.Teachers?.Count(t => t.Availabilities?.Any() == true) ?? 0,
                WithSubjects = school.Teachers?.Count(t => t.Subjects?.Any() == true) ?? 0,
                Details = school.Teachers?.Select(t => new
                {
                    t.TeacherId,
                    AvailabilitySlots = t.Availabilities?.Count ?? 0,
                    Subjects = t.Subjects?.Count ?? 0,
                    SubjectIds = t.Subjects?.Select(s => s.SubjectId).ToList() ?? []
                }).ToList()
            },

            Classrooms = new
            {
                Total = school.Classrooms?.Count ?? 0,
                Details = school.Classrooms?.Select(c => new
                {
                    c.ClassroomId,
                    c.ClassName,
                    c.SchoolLevelId
                }).ToList()
            },

            Timeslots = new
            {
                Total = school.Timeslots?.Count ?? 0,
                ByDay = school.Timeslots?.GroupBy(t => t.Day)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()) ?? []
            },

            LevelSubjectHours = new
            {
                Total = levelSubjectHours.Count,
                Details = levelSubjectHours.Select(lsh => new
                {
                    lsh.LevelId,
                    lsh.SchoolLevel?.LevelYear,
                    lsh.SubjectId,
                    SubjectName = lsh.Subject?.Name,
                    lsh.HoursPerWeek,
                    lsh.Priority
                }).ToList()
            },

            Issues = new List<string>()
        };

        var issues = detailedValidation.Issues;

        if (school.ScheduleConfig == null)
            issues.Add("Missing schedule configuration");

        if (detailedValidation.Groups.Total == 0)
            issues.Add("No groups found");
        else if (detailedValidation.Groups.WithClassrooms == 0)
            issues.Add("No groups have classrooms assigned");
        else if (detailedValidation.Groups.WithSchoolLevel == 0)
            issues.Add("No groups have school levels assigned through classrooms");

        if (detailedValidation.Teachers.Total == 0)
            issues.Add("No teachers found");
        else
        {
            if (detailedValidation.Teachers.WithAvailability == 0)
                issues.Add("No teachers have availability set");
            if (detailedValidation.Teachers.WithSubjects == 0)
                issues.Add("No teachers have subjects assigned");
        }

        if (detailedValidation.Classrooms.Total == 0)
            issues.Add("No classrooms found");

        if (detailedValidation.Timeslots.Total == 0)
            issues.Add("No timeslots generated - configure school schedule first");

        if (detailedValidation.LevelSubjectHours.Total == 0)
            issues.Add("No subject hours configured");

        // Check for matching between groups and level subject hours
        var groupLevels = school.Groups?.Where(g => g.Classroom?.SchoolLevelId != null)
            .Select(g => g.Classroom!.SchoolLevelId).Distinct().ToList() ?? [];
        var configuredLevels = levelSubjectHours.Select(lsh => lsh.LevelId).Distinct().ToList();

        var missingLevels = groupLevels.Except(configuredLevels).ToList();
        if (missingLevels.Count != 0)
            issues.Add($"Groups exist for levels {string.Join(", ", missingLevels)} but no subject hours configured for these levels");

        // Check for teacher-subject matching
        var requiredSubjects = levelSubjectHours.Select(lsh => lsh.SubjectId).Distinct().ToList();
        var teacherSubjects = school.Teachers?.SelectMany(t => t.Subjects?.Select(s => s.SubjectId) ?? new List<int>()).Distinct().ToList() ?? [];

        var missingSubjects = requiredSubjects.Except(teacherSubjects).ToList();
        if (missingSubjects.Count != 0)
            issues.Add($"No teachers assigned for subjects: {string.Join(", ", missingSubjects)}");

        return Ok(detailedValidation);
    }

    [HttpGet("schools/{schoolId}/schedule")]
    public async Task<IActionResult> GetSchedule(
        [FromRoute, Required] Guid schoolId)
    {
        try
        {
            var schedule = await _context.Lessons
                .Include(l => l.Timeslot)
                .Include(l => l.Teacher).ThenInclude(t => t!.User)
                .Include(l => l.Classroom)
                .Include(l => l.Subject)
                .Include(l => l.Group)
                .Where(l => l.SchoolId == schoolId)
                .ToListAsync();

            if (schedule.Count == 0)
                return NotFound(new { Error = "No schedule found for given parameters" });

            return Ok(new
            {
                Success = true,
                TotalLessons = schedule.Count,
                schedule = FormatSchedule(schedule)
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

            // Remove existing availability
            if (teacher.Availabilities != null)
            {
                _context.TeacherAvailabilities.RemoveRange(teacher.Availabilities.Where(a => a != null)!);
            }

            var newAvailability = availability.Select(a => new TeacherAvailability
            {
                TeacherId = teacherId,
                Day = a.Day,
                StartTime = a.StartTime,
                EndTime = a.EndTime
            }).ToList();

            await _context.TeacherAvailabilities.AddRangeAsync(newAvailability);
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
        var groupedSchedule = lessons
            .Where(l => l.Timeslot != null)
            .GroupBy(l => l.Timeslot!.Day)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Day = g.Key.ToString(),
                Lessons = g.OrderBy(l => l.Timeslot!.StartTime)
                           .Select(l => new
                           {
                               Subject = l.Subject?.Name ?? "Unknown",
                               Teacher = l.Teacher?.User != null
                                   ? $"{l.Teacher.User.FirstName} {l.Teacher.User.LastName}"
                                   : "Unknown",
                               TeacherId = l.Teacher?.TeacherId ?? Guid.Empty,
                               Classroom = l.Classroom?.ClassName ?? "Unknown",
                               Time = l.Timeslot != null
                                   ? $"{l.Timeslot.StartTime:hh\\:mm} - {l.Timeslot.EndTime:hh\\:mm}"
                                   : "Unknown",
                               Group = l.GroupId
                           })
            });

        return groupedSchedule;
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

            _context.LevelSubjectHours.RemoveRange(existing);

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

internal class ScheduleConfigResponseDto
{
    public int ConfigId { get; set; }
    public Guid SchoolId { get; set; }
    public TimeSpan MorningStart { get; set; }
    public TimeSpan MorningEnd { get; set; }
    public TimeSpan AfternoonStart { get; set; }
    public TimeSpan AfternoonEnd { get; set; }
    public required DayOfWeek[] FullDays { get; set; }
    public required DayOfWeek[] ShortDays { get; set; }
    public required DayOfWeek[] DaysOff { get; set; }
}