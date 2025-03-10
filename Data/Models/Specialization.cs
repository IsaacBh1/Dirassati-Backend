using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Domain.Models;

public partial class Specialization
{
    [Key]

    public int SpecializationId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<School> Schools { get; set; } = new List<School>();

}
