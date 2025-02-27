using System;
using System.Collections.Generic;

namespace Dirassati_Backend.Domain.Models;

public partial class Teach
{
    public int TeacherId { get; set; }

    public int GroupId { get; set; }

    public int SubjectId { get; set; }
}
