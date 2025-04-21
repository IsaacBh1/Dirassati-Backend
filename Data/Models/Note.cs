using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dirassati_Backend.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Data.Models
{
    public class Note
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int NoteId { get; set; }

        public Guid StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public Student? Student { get; set; }

        [Range(1, 3)]
        public int Tremester { get; set; }
        public Guid TeacherId { get; set; }
        [ForeignKey(nameof(TeacherId))]
        public Teacher? Teacher { get; set; }

        public int AcademicYearId { get; set; }
        [ForeignKey(nameof(AcademicYearId))]
        public AcademicYear? AcademicYear { get; set; }

        public int ExamTypeId { get; set; }
        [ForeignKey(nameof(ExamTypeId))]
        public ExamType? ExamType { get; set; }

        public int SubjectId { get; set; }
        [ForeignKey(nameof(SubjectId))]
        public Subject? Subject { get; set; }
        [Range(0, 20)]
        [Column(TypeName = "decimal(5, 2)")]
        [Precision(5, 2)]
        public double Value { get; set; }
        public Guid SchoolId { get; set; }
        [ForeignKey(nameof(SchoolId))]
        public School? School { get; set; }
        public Guid GroupId { get; set; }
        [ForeignKey(nameof(GroupId))]
        public Group? Group { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    
}