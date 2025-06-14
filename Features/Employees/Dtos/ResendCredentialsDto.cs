
using System.ComponentModel.DataAnnotations;


namespace Dirassati_Backend.Features.Employees.Dtos
{

    public class ResendCredentialsDto
    {
        [Required]
        public Guid EmployeeId { get; set; }
    }
}