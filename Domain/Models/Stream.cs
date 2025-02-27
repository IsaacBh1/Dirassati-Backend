using System;
using System.Collections.Generic;

namespace Dirassati_Backend.Domain.Models;

public partial class Stream
{
    public int StreamId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<SchoolLevel> SchoolLevels { get; set; } = new List<SchoolLevel>();
}
