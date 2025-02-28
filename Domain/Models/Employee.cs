using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Domain.Models;

public partial class Employee
{
    [Key]
    public Guid EmployeeId { get; set; } = new Guid();

    public Guid UserId { get; set; }

    public string Position { get; set; } = string.Empty;

    public string HireDate { get; set; } = string.Empty;

    public string ContractType { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int Permissions { get; set; }

    public string SchoolId { get; set; } = string.Empty;

    public virtual School School { get; set; }=null!; 

    public virtual AppUser User { get; set; }  =null!; 
}
