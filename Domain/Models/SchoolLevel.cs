using System;
using System.Collections.Generic;

namespace Dirassati_Backend.Domain.Models;

public partial class SchoolLevel
{
    public int LevelId { get; set; }

    public string LevelType { get; set; } = null!;

    public int LevelYear { get; set; }

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<Stream> Streams { get; set; } = new List<Stream>();
}
