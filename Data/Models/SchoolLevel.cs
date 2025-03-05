using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dirassati_Backend.Data.Models;

namespace Dirassati_Backend.Domain.Models;

public partial class SchoolLevel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LevelId { get; set; }

    [Required]
    [ForeignKey(nameof(SchoolType))]
    public int SchoolTypeId { get; set; }

    [Required]
    [Range(1, 12)] // Assuming years 1-12 for school levels
    public int LevelYear { get; set; }

    // Navigation properties
    public virtual SchoolType SchoolType { get; set; } = null!;

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<Specialization> Specializations { get; set; } = new List<Specialization>();
}