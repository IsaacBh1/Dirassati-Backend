using Dirassati_Backend.Common.Dtos;

namespace Dirassati_Backend.Features.Auth.Register.Dtos;

public class SchoolDto
{
    public string Name { get; set; } = string.Empty;
    public int SchoolTypeId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public AddressDto Address { get; set; } = null!;
    public List<int>? SpecializationsId { get; set; }
    public string Logo { get; set; } = null!;
    public string WebsiteUrl { get; set; } = null!;
    public AcademicYearDto AcademicYear { get; set; } = null!;


}
