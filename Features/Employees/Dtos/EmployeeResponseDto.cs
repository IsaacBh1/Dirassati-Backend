namespace Dirassati_Backend.Features.Employees.Dtos
{


    public class EmployeeResponseDto
    {
        public Guid EmployeeId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public DateOnly BirthDate { get; set; }
        public string Position { get; set; } = string.Empty;
        public DateOnly HireDate { get; set; }
        public string ContractType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int Permissions { get; set; }
        public string? PhoneNumber { get; set; }

        // Address Information
        public AddressDto? Address { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }
    }

}