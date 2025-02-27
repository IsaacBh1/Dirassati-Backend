using System;
using System.Collections.Generic;

namespace Dirassati_Backend.Domain.Models;

public partial class PhoneNumber
{
    public int PhoneNumberId { get; set; }

    public string PhoneNumber1 { get; set; } = null!;

    public string SchoolId { get; set; } = null!;

    public virtual School School { get; set; } = null!;
}
