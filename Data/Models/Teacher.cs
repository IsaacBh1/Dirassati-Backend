﻿using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

using Dirassati_Backend.Domain.Models;


public partial class Teacher
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

    public Guid TeacherId { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
    public int ContractTypeId { get; set; }
    public Guid SchoolId { get; set; }
    public virtual ContractType ContractType { get; set; } = null!;
    public virtual School School { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;
    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
