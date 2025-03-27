using Dirassati_Backend.Common.Dtos;
using Dirassati_Backend.Data.Models;

namespace Dirassati_Backend.Features.School.DTOs;

public class GetSchoolInfoDTO
{
    public Guid SchoolId { get; set; } = Guid.NewGuid();

    public required string Name { get; set; } = null!;

    public AdressDto Address { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Logo { get; set; } = null!;

    public string WebsiteUrl { get; set; } = null!;

    public string SchoolConfig { get; set; } = null!;
    public virtual List<string> PhoneNumbers { get; set; } = [];

    public SchoolType SchoolType { get; set; } = null!;


    public AcademicYearDto? AcademicYear { get; set; } = null!;
}
