namespace Dirassati_Backend.Features.Students.DTOs;

public class StudentImportResultDto
{
    public int TotalRecords { get; set; }
    public int SuccessfulImports { get; set; }
    public int FailedImports { get; set; }
    public List<string> Errors { get; set; } = [];
}