using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dirassati_Backend.Data.Models;

public partial class Employee
{
    [Key]
    public Guid EmployeeId { get; set; } = Guid.NewGuid();
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;
    public string Position { get; set; } = string.Empty;
    public DateOnly HireDate { get; set; }
    public string ContractType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int Permissions { get; set; }

    [ForeignKey(nameof(School))]
    public Guid SchoolId { get; set; } = Guid.Empty;
    public virtual School School { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;
}
