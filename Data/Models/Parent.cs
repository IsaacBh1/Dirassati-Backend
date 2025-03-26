using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Data.Models;

public partial class Parent
{
    [Key]
    public Guid ParentId { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string Occupation { get; set; } = null!;
    public string NationalIdentityNumber { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;
}
