using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Domain.Models;

public partial class ParentRelationshipToStudentType
{
    [Key]

    public int Id { get; set; }

    public string Name { get; set; } = null!;


    public virtual ICollection<Student> Students { get; set; } = [];

}
