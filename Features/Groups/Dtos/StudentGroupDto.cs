namespace Dirassati_Backend.Features.Groups.Dtos
{
    public class StudentGroupDto
    {
        public Guid StudentId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string StudentIdNumber { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public string ParentContact { get; set; } = string.Empty;
        public string ParentEmail { get; set; } = string.Empty;
    }
    public class GetGroupStudetDto
    {
        public Guid StudentId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
      
    }
 
}