using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dirassati_Backend.Domain.Models;

public partial class Teach
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public Guid TeacherId { get; set; }
    public int GroupId { get; set; }

    public int SubjectId { get; set; }
    public Teacher? Teacher { get; set; }
    public Group? Group { get; set; }
    public Subject? Subject { get; set; }

}
