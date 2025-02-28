using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dirassati_Backend.Domain.Models;

public partial class SchoolLevel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LevelId { get; set; }

    public string LevelType { get; set; } = null!;

    public int LevelYear { get; set; }

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<Specialization> Streams { get; set; } = new List<Specialization>();
}
