namespace Dirassati_Backend.Features.Parents.Dtos
{
    public class GetStudentDto
    {
        public Guid StudentId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateOnly EnrollmentDate { get; set; }
        public string Grade { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }


}