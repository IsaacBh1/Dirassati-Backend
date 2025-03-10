using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dirassati_Backend.Domain.Models;

public partial class AcademicYear
{
    public DateOnly StartDate { get; set; }
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AcademicYearId { get; set; }
    public DateOnly EndDate { get; set; }


    public Guid SchoolId { get; set; }

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual School School { get; set; } = null!;
}
