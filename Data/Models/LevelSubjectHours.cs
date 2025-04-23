using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Dirassati_Backend.Data.Models
{
    public class LevelSubjectHours
    {
        [Key]
        public int LevelId { get; set; }
        [Key]
        public int SubjectId { get; set; }

        [Required]
        public Guid SchoolId { get; set; }

        public int HoursPerWeek { get; set; }

        [ForeignKey(nameof(SchoolId))]
        public School? School { get; set; }

        public SchoolLevel? SchoolLevel { get; set; }
        public Subject? Subject { get; set; }

        public int Priority { get; set; } = 1;
    }
}