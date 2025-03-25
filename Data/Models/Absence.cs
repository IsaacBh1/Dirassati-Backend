using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Data.Models;

public partial class Absence
{
    [Key]
    public Guid AbsenceId { get; set; } = Guid.Empty;
    public DateTime DateTIme { get; set; } = DateTime.Now;
    public Guid StudentId { get; set; }
    public bool IsJustified { get; set; }
    public string Remark { get; set; } = null!;
    public bool IsNotified { get; set; }
    public virtual Student? Student { get; set; }
}
