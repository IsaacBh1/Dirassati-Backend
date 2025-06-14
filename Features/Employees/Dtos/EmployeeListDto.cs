
namespace Dirassati_Backend.Features.Employees.Dtos
{
    public class EmployeeListDto
    {
        public Guid EmployeeId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateOnly HireDate { get; set; }
        public string ContractType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int Permissions { get; set; }
    }
}