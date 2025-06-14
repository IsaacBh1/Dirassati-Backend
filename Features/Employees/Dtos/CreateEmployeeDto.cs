using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Features.Employees.Dtos
{
    public class CreateEmployeeDto
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public DateOnly BirthDate { get; set; }

        [Required]
        [StringLength(100)]
        public string Position { get; set; } = string.Empty;

        [Required]
        public DateOnly HireDate { get; set; }

        [Required]
        [StringLength(50)]
        public string ContractType { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Permissions { get; set; }

        public bool IsActive { get; set; } = true;

        // Address Information
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }
    }







}