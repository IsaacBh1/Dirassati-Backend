using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Domain.Models;

public partial class Student
{
    [Key]
    public Guid StudentId { get; set; } = new Guid();
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string BirthDate { get; set; } = null!;
    public string BirthPlace { get; set; } = null!;
    public Guid SchoolId { get; set; } = Guid.Empty; // note : we fogot to add this when we created the models
    public virtual School School { get; set; } = null!; // note : we fogot to add this when we created the models
    public string? StudentIdNumber { get; set; }
    public string EmergencyContact { get; set; } = null!;
    public int AcademicYearId { get; set; }
    public byte[]? PhotoURL;
    public DateOnly EnrollmentDate { get; set; }
    public Guid ParentId { get; set; } = Guid.Empty;
    public Parent parent { get; set; } = null!;
    public bool IsActive { get; set; }
    public int StreamId { get; set; }
    public byte LevelYear { get; set; }
    public virtual ICollection<Absence> Absences { get; set; } = new List<Absence>();
    public virtual Specialization Stream { get; set; } = null!;
    public virtual AcademicYear AcademicYear { get; set; } = null!;
}
