using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Features.Teachers.Dtos
{
    public class TeacherInfosDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required]
        public DateOnly HireDate { get; set; }
        [Required]
        public int ContractTypeId { get; set; }
        public string ContractType { get; set; } = "";

        public List<int> SubjectIds { get; set; } = [];
        public Guid? SchoolId { get; set; }
        public byte[]? Photo { get; set; }
        public TeacherAddressDto? Address { get; set; }
    }

    public class TeacherAddressDto
    {
        public string? FullAddress { get; set; }
    }
}