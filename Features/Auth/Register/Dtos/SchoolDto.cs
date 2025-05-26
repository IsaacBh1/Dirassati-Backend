using Dirassati_Backend.Common.Dtos;

namespace Dirassati_Backend.Features.Auth.Register.Dtos;

using System.ComponentModel.DataAnnotations;

public class SchoolDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public  int SchoolTypeId { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]

    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public AddressDto Address { get; set; } = null!;

    public List<int>? SpecializationsId { get; set; }



    public string? LogoUrl { get; set; }


    public string? WebsiteUrl { get; set; }

    public string? BankCode { get; set; }
    public AcademicYearDto AcademicYear { get; set; } = null!;

    [Range(0, double.MaxValue)]
    public decimal BillAmount { get; set; }
}
