using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Domain.Models;

public partial class School
{
    [Key]
    public Guid SchoolId { get; set; } = new Guid();

    public required string Name { get; set; } = null!;

    public int AddressId { get; set; }

    public string Email { get; set; } = null!;

    public string Logo { get; set; } = null!;

    public string WebsiteUrl { get; set; } = null!;

    public string SchoolConfig { get; set; } = null!;

    public string SchoolType { get; set; } = null!;
    public Address Address { get; set; } = new Address();

    public virtual ICollection<AcademicYear> AcademicYears { get; set; } = new List<AcademicYear>();

    public virtual ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; } = new List<PhoneNumber>();

    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}
