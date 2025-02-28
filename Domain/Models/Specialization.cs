using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dirassati_Backend.Domain.Models;

public partial class Specialization
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int StreamId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<SchoolLevel> SchoolLevels { get; set; } = new List<SchoolLevel>();
}
