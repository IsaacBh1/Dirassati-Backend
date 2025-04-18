using System.ComponentModel.DataAnnotations;
using Dirassati_Backend.Data.Models;

namespace Dirassati_Backend.Features.Scheduling.Dtos;

public class SchoolConfigRequest
{
    [Required] public TimeSpan MorningStart { get; set; }
    [Required] public TimeSpan MorningEnd { get; set; }
    [Required] public TimeSpan AfternoonStart { get; set; }
    [Required] public TimeSpan AfternoonEnd { get; set; }
    public DayOfWeek[] DaysOff { get; set; } = new DayOfWeek[0];
    public DayOfWeek[] ShortDays { get; set; } = new DayOfWeek[0];
}

public class SubjectPriorityRequest
{
    [Required] public int LevelId { get; set; }
    [Required] public int SubjectId { get; set; }
    [Range(1, 5)] public int Priority { get; set; }
}
public class ScheduleResult
{
    public List<Lesson> TeacherSchedules { get; set; } = new();
    public List<Lesson> GroupSchedules { get; set; } = new();
    public int TotalConflicts { get; set; }
    public List<SubjectHoursStatus> HoursCompliance { get; set; } = new();
}

public class SubjectHoursStatus
{
    public int LevelId { get; set; }
    public int SubjectId { get; set; }
    public int RequiredHours { get; set; }
    public int ScheduledHours { get; set; }
    public bool IsFulfilled => ScheduledHours >= RequiredHours;
}