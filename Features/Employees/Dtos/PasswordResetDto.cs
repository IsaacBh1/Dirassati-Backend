using System.ComponentModel.DataAnnotations;
namespace Dirassati_Backend.Features.Employees.Dtos
{
    public class PasswordResetDto
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}