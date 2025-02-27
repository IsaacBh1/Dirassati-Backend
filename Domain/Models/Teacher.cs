using System;
using System.Collections.Generic;

namespace Dirassati_Backend.Domain.Models;

public partial class Teacher
{
    public string TeacherId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string HireDate { get; set; } = null!;

    public int ContractTypeId { get; set; }

    public string SchoolId { get; set; } = null!;

    public virtual ContractType ContractType { get; set; } = null!;

    public virtual School School { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;

    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
