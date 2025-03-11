using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dirassati_Backend.Domain.Models;

public partial class Classroom
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ClassroomId { get; set; }

    public string ClassName { get; set; } = null!;

    public Guid SchoolId { get; set; } = Guid.Empty;

    public virtual School School { get; set; } = null!;
}
