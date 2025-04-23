using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Dirassati_Backend.Data.Models
{
    public class Lesson
    {
        public int LessonId { get; set; }
        [Required]
        public Guid SchoolId { get; set; }
        public int TimeslotId { get; set; }
        public Guid ClassroomId { get; set; }
        public Guid GroupId { get; set; }
        public int SubjectId { get; set; }
        public Guid TeacherId { get; set; }
        public int AcademicYearId { get; set; }

        [ForeignKey(nameof(SchoolId))]
        public School? School { get; set; }
        public Timeslot? Timeslot { get; set; }
        public Classroom? Classroom { get; set; }
        public Group? Group { get; set; }
        public Teacher? Teacher { get; set; }
        public Subject? Subject { get; set; }
    }
}