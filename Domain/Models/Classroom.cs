using System;
using System.Collections.Generic;

namespace Dirassati_Backend.Domain.Models;

public partial class Classroom
{
    public int ClassroomId { get; set; }

    public string ClassName { get; set; } = null!;

    public string SchoolId { get; set; } = null!;

    public virtual School School { get; set; } = null!;
}
