using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dirassati_Backend.Data.Models;

public partial class Group
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int GroupId { get; set; }
    public string GorupName { get; set; } = null!;
    public int LevelId { get; set; }
    public int? AcademicYearId { get; set; }
    public int GroupCapacity { get; set; }
    public Guid SchoolId { get; set; } = Guid.Empty;
    public int? StreamId { get; set; }
    public virtual AcademicYear? AcademicYear { get; set; }
    public virtual SchoolLevel Level { get; set; } = null!;
    public virtual School School { get; set; } = null!;
    public virtual Specialization? Stream { get; set; }
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    
}
