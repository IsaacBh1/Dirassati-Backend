namespace Dirassati_Backend.Features.Students.DTOs;

public class AddStudentDTO
{
    public required StudentInfosDTO studentInfosDTO { get; set; }
    public required ParentInfosDTO parentInfosDTO { get; set; }
}
