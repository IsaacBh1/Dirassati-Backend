using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dirassati_Backend.Data.Models;

public partial class SchoolLevel
{
    [Key]
    public int LevelId { get; set; }

    [ForeignKey(nameof(SchoolType))]
    public int SchoolTypeId { get; set; }
    [Range(1, 5)]
    public int LevelYear { get; set; }

    // Navigation properties
    public virtual SchoolType SchoolType { get; set; } = null!;
    public virtual ICollection<Group> Groups { get; set; } = [];
    public virtual ICollection<Bill> Bills { get; set; } = [];



}
