using System.ComponentModel.DataAnnotations;
using Dirassati_Backend.Common.Dtos;




namespace Dirassati_Backend.Features.School.DTOs;

public class UpdateSchoolInfosDTO
{
    [Required]

    public required string Name { get; set; } = null!;
    [Required]

    public string Email { get; set; } = null!;
    [Required]

    public string Logo { get; set; } = null!;
    [Required]

    public string WebsiteUrl { get; set; } = null!;

    [Required]
    public AcademicYearDto AcademicYear { get; set; } = null!;
    [Required]

    public List<PhoneNumberDTO> PhoneNumbers { get; set; } = null!;
    [Required]

    public AddressDto Address { get; set; } = null!;
    [Required]

    public List<int> Specializations { get; set; } = null!;


}

