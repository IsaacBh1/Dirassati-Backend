﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dirassati_Backend.Data.Models;

public partial class Student
{
    [Key]
    public Guid StudentId { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public DateOnly BirthDate { get; set; }
    public string BirthPlace { get; set; } = null!;
    public Guid SchoolId { get; set; } = Guid.Empty;
    public virtual School School { get; set; } = null!;
    public string? StudentIdNumber { get; set; }
    public string EmergencyContact { get; set; } = null!;

    [ForeignKey(nameof(SchoolLevel))]
    public int SchoolLevelId { get; set; }
    [ForeignKey(nameof(Specialization))]
    public int? SpecializationId { get; set; }
    public int ParentRelationshipToStudentTypeId { get; set; }
    public byte[]? PhotoUrl { get; set; }
    public DateOnly EnrollmentDate { get; set; }

    [ForeignKey("Parent")]
    public Guid ParentId { get; set; }
    public bool IsActive { get; set; } = true;

    [ForeignKey("Group")]
    public Guid? GroupId { get; set; }
    public virtual Group? Group { get; set; }
    public ParentRelationshipToStudentType ParentRelationshipToStudentType { get; set; } = null!;
    public virtual SchoolLevel SchoolLevel { get; set; } = null!;
    public virtual Specialization? Specialization { get; set; }
    public virtual Parent Parent { get; set; } = null!;
    public virtual ICollection<Absence> Absences { get; set; } = [];
    public virtual ICollection<StudentReport> StudentReports { get; set; } = [];
    public virtual ICollection<StudentPayment> StudentPayments { get; set; } = [];


}
