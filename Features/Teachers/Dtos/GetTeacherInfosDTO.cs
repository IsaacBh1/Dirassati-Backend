using Dirassati_Backend.Data.Models;

namespace Dirassati_Backend.Features.Teachers.Dtos
{
    public class GetTeacherInfosDto
    {

        public string TeacherId { get; set; } = null!;
        public string Email { get; set; } = string.Empty;


        public string FirstName { get; set; } = string.Empty;


        public string LastName { get; set; } = string.Empty;


        public string PhoneNumber { get; set; } = string.Empty;


        public DateOnly HireDate { get; set; }


        public int ContractTypeId { get; set; }
        public string ContractType { get; set; } = "";

        public List<int> SubjectIds { get; set; } = new List<int>();

        public Guid SchoolId { get; set; }

        public byte[]? Photo { get; set; }

        public Address? Address { get; set; }
    }
}