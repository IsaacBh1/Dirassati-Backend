using System.ComponentModel.DataAnnotations;
namespace Dirassati_Backend.Data.Models;

public class SchoolScheduleConfig
{
    [Key]
    public int ConfigId { get; set; }
    public Guid SchoolId { get; set; }

    // Regular hours
    public TimeSpan MorningStart { get; set; } = new TimeSpan(8, 0, 0);
    public TimeSpan MorningEnd { get; set; } = new TimeSpan(12, 0, 0);
    public TimeSpan AfternoonStart { get; set; } = new TimeSpan(13, 0, 0);
    public TimeSpan AfternoonEnd { get; set; } = new TimeSpan(16, 0, 0);

    public DayOfWeek[] FullDays { get; set; }
    public DayOfWeek[] ShortDays { get; set; }
    public DayOfWeek[] DaysOff { get; set; }

    public SchoolScheduleConfig()
    {
        FullDays = new[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday };
        ShortDays = new[] { DayOfWeek.Thursday };
        DaysOff = new[] { DayOfWeek.Friday, DayOfWeek.Saturday };
    }
}
