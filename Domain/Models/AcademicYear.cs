using System;
using System.Collections.Generic;

namespace Dirassati_Backend.Domain.Models;

public partial class AcademicYear
{
    public string StartDate { get; set; } = null!;

    public int AcademicYearId { get; set; }

    public string EndDate { get; set; } = null!;

    public string SchoolId { get; set; } = null!;

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual School School { get; set; } = null!;
}
