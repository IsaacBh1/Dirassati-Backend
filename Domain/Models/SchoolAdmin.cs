using System;
using System.Collections.Generic;

namespace Dirassati_Backend.Domain.Models;

public partial class SchoolAdmin
{
    public string SchoolAdminId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string SchoolId { get; set; } = null!;

    public virtual School School { get; set; } = null!;

    public virtual ICollection<School> Schools { get; set; } = new List<School>();

    public virtual AspNetUser User { get; set; } = null!;
}
