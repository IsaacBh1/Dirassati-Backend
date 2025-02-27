using System;
using System.Collections.Generic;

namespace Dirassati_Backend.Domain.Models;

public partial class Group
{
    public int GroupId { get; set; }

    public string GorupName { get; set; } = null!;

    public int LevelId { get; set; }

    public int? AcademicYearId { get; set; }

    public int GroupCapacity { get; set; }

    public string SchoolId { get; set; } = null!;

    public int? StreamId { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }

    public virtual SchoolLevel Level { get; set; } = null!;

    public virtual School School { get; set; } = null!;

    public virtual Stream? Stream { get; set; }
}
