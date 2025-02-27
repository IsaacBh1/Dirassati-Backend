using System;
using System.Collections.Generic;

namespace Dirassati_Backend.Domain.Models;

public partial class School
{
    public string SchoolId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Logo { get; set; } = null!;

    public string WebsiteUrl { get; set; } = null!;

    public string SchoolConfig { get; set; } = null!;

    public string AdminId { get; set; } = null!;

    public virtual ICollection<AcademicYear> AcademicYears { get; set; } = new List<AcademicYear>();

    public virtual SchoolAdmin Admin { get; set; } = null!;

    public virtual ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; } = new List<PhoneNumber>();

    public virtual ICollection<SchoolAdmin> SchoolAdmins { get; set; } = new List<SchoolAdmin>();

    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}
