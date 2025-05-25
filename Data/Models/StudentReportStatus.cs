namespace Dirassati_Backend.Data.Models;


public class StudentReportStatus
{
    public int StudentReportStatusId { get; set; }
    public string Name { get; set; } = null!;
    public virtual ICollection<StudentReport> StudentReports { get; set; } = [];

}