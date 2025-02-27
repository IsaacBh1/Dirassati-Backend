using System;
using System.Collections.Generic;

namespace Dirassati_Backend.Domain.Models;

public partial class Adress
{
    public int AdresseId { get; set; }

    public string City { get; set; } = null!;

    public string? State { get; set; }

    public string Country { get; set; } = null!;
}
