using Dirassati_Backend.Common.Dtos;

namespace Dirassati_Backend.Features.Teachers.Dtos;

public class GetStudentReportDto
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public Guid StudentId { get; set; }

    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime ReportDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string Subject { get; set; } = null!;
    public SimpleTeacherDto Teacher { get; set; } = null!;
    public SimpleStudentDto Student { get; set; } = null!;
}
