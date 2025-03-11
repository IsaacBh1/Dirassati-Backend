namespace Dirassati_Backend.Features.Students.DTOs;

public class StudentInfosDTO
{


    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public DateOnly BirthDate { get; set; }

    public string BirthPlace { get; set; } = null!;

    public string EmergencyContact { get; set; } = null!;

    public int SchoolLevelId { get; set; }

    public int? SpecializationId { get; set; }



}
