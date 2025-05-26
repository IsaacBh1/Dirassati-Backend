using Dirassati_Backend.Common.Dtos;
using Dirassati_Backend.Data.Models;

namespace Dirassati_Backend.Features.School.DTOs;

public class GetSchoolInfoDto
{
    public Guid SchoolId { get; set; } = Guid.NewGuid();

    public required string Name { get; set; } = null!;

    public AddressDto Address { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string LogoUrl { get; set; } = null!;

    public string WebsiteUrl { get; set; } = null!;

    public string SchoolConfig { get; set; } = null!;
    public virtual List<PhoneNumberDto> PhoneNumbers { get; set; } = [];
    public virtual List<SpecializationDto> Specializations { get; set; } = [];

    public SchoolType SchoolType { get; set; } = null!;


    public Dirassati_Backend.Common.Dtos.AcademicYearDto? AcademicYear { get; set; } = null!;
}
