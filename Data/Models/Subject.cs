using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dirassati_Backend.Data.Enums;

namespace Dirassati_Backend.Data.Models;

public partial class Subject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SubjectId { get; set; }
    public string Name { get; set; } = null!;
    public SchoolTypeEnum SchoolType { get; set; }
    public virtual ICollection<Teacher> Teachers { get; set; } = [];
}
