namespace Dirassati_Backend.Features.Students.DTOs;

public class ParentInfosDTO
{
    public string NationalIdentityNumber { get; set; } = null!;
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = null!;
    public int RelationshipToStudentId { get; set; }
    public string Occupation { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;

}
