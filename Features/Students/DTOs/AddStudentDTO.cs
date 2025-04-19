namespace Dirassati_Backend.Features.Students.DTOs;

public class AddStudentDto
{
    public required StudentInfosDto studentInfosDTO { get; set; }
    public required ParentInfosDto parentInfosDTO { get; set; }
}
