using System.ComponentModel.DataAnnotations;
using Dirassati_Backend.Common.Dtos;

namespace Dirassati_Backend.Features.Auth.Register.Dtos;

public class RegisterDto
{
    public required SchoolDto School { get; set; }
    public required EmployeeDto Employee { get; set; }
}

public class ImprovedRegisterDto
{
    //School Informations
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string SchoolName { get; set; } = string.Empty;

    [Required]
    public  int SchoolTypeId { get; set; }

    [Required]
    [EmailAddress]
    public string SchoolEmail { get; set; } = string.Empty;

    [Required]
    public string SchoolPhoneNumber { get; set; } = string.Empty;

    //Address Information
    [Required]
    [MaxLength(100)]
    public string Street { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string City { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string State { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Country { get; set; } = null!;
    
    [Required]
    [MaxLength(20)]
    public string PostalCode { get; set; } = null!;
    
    public List<int>? SpecializationsId { get; set; }
    
    public IFormFile? SchoolLogo { get; set; }

    public string BankCode { get; set; } = null!;
    public string? WebsiteUrl { get; set; }
    //Academic Year Information
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal BillAmount { get; set; }
    
    //Employee Informations
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Password { get; set; }
    public IFormFile? ProfilePicture { get; set; }
    public int Permission { get; set; }
}