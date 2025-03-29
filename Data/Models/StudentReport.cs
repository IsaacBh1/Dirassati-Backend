using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Data.Models;

public class StudentReport
{
    [Key]
    public Guid StudentReportId { get; set; } = Guid.NewGuid();
    public Guid TeacherId { get; set; }
    public Guid StudentId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int StudentReportStatusId { get; set; } = (int)Enums.ReportStatusEnum.Pending;
    public virtual Teacher Teacher { get; set; } = null!;
    public virtual Student Student { get; set; } = null!;
    public virtual StudentReportStatus StudentReportStatus { get; set; } = null!;


}