namespace Dirassati_Backend.Features.Auth.Register.Dtos;

public class SchoolDto
{
    public string SchoolName { get; set; } = string.Empty;
    public int SchoolTypeId { get; set; }
    public string SchoolEmail { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public RegisterAddressDto Address { get; set; } = null!;
    public List<int>? SpecializationsId { get; set; }
}
