namespace Dirassati_Backend.Features.Teachers.Dtos;

public class AddStudentReportDto
{
    public Guid StudentId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;

}
