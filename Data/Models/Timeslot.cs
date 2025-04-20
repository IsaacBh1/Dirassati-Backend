using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Dirassati_Backend.Data.Models
{
    public class Timeslot
    {
        public int TimeslotId { get; set; }

        [Required]
        public Guid SchoolId { get; set; }

        public DayOfWeek Day { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        [ForeignKey(nameof(SchoolId))]
        public School School { get; set; } = null!;
        public bool IsMorningSlot { get; set; }  
        public bool IsSpecialDay { get; set; }   
    }
}