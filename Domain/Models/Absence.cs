using System;
using System.Collections.Generic;

namespace Dirassati_Backend.Domain.Models;

public partial class Absence
{
    public Guid AbsenceId { get; set; } =Guid.Empty;

    public DateTime DateTime { get; set; }

    public string? StudentId { get; set; }

    public bool IsJustified { get; set; }

    public string Remark { get; set; } = null!;

    public virtual Student? Student { get; set; }
}
