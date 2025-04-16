using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dirassati_Backend.Data.Models;

public partial class Classroom
{
    [Key]
    public Guid ClassroomId { get; set; }

    public string ClassName { get; set; } = null!;

    public Guid SchoolId { get; set; } = Guid.Empty;
    public int SchoolLevelId { get; set; }

    public virtual School School { get; set; } = null!;
    public virtual SchoolLevel SchoolLevel { get; set; } = null!;
    public virtual ICollection<Group> Groups { get; set; } = [];
}
