using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dirassati_Backend.Data.Models
{
    public partial class Group
    {
        [Key]
        public Guid GroupId { get; set; } = Guid.NewGuid(); // Primary Key for the Group

        [Required] // Added required attribute for clarity
        public string GroupName { get; set; } = null!; // Name of the group

        public int GroupCapacity { get; set; } // Maximum number of students in the group

        [Required] // Added required attribute
        [ForeignKey(nameof(School))]
        public Guid SchoolId { get; set; } = Guid.Empty; // Foreign Key referencing the School

        public Guid ClassroomId { get; set; }

        // Navigation properties
        public virtual School School { get; set; } = null!;
        public virtual ICollection<Student> Students { get; set; } = [];
        public Classroom? Classroom { get; set; }
    }
}