using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Features.Abcenses.Dtos
{
    public class AbsenceDto
    {
        public Guid AbsenceId { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsJustified { get; set; }
        public string Remark { get; set; } = string.Empty;
        public bool IsNotified { get; set; }
    }

    public class StudentAbsenceDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public List<AbsenceDto> Absences { get; set; } = new List<AbsenceDto>();
    }

    public class GetStudentAbsencesRequestDto
    {
        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid ParentId { get; set; }
    }
}
