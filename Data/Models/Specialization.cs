using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Data.Models;

public partial class Specialization
{
    [Key]

    public int SpecializationId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Group> Groups { get; set; } = [];

    public virtual ICollection<Student> Students { get; set; } = [];

    public virtual ICollection<School> Schools { get; set; } = [];

}
