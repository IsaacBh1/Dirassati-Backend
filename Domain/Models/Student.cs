using System;
using System.Collections.Generic;

namespace Dirassati_Backend.Domain.Models;

public partial class Student
{
    public string StudentId { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string BirthDate { get; set; } = null!;

    public string BirthPlace { get; set; } = null!;

    public string? StudentIdNumber { get; set; }

    public string EmergencyContact { get; set; } = null!;

    public int AcademicYear { get; set; }

    public string? PhotoURL { get; set; }

    public string EnrollmentDate { get; set; } = null!;

    public string ParentId { get; set; } = null!;

    public int IsActive { get; set; }

    public int StreamId { get; set; }

    public virtual ICollection<Absence> Absences { get; set; } = new List<Absence>();

    public virtual Stream Stream { get; set; } = null!;
}
