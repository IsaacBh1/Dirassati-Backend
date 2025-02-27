using System;
using System.Collections.Generic;

namespace Dirassati_Backend.Domain.Models;

public partial class Employee
{
    public string EmployeeId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string Position { get; set; } = null!;

    public string HireDate { get; set; } = null!;

    public string ContractType { get; set; } = null!;

    public int IsActive { get; set; }

    public virtual AspNetUser User { get; set; } = null!;
}
