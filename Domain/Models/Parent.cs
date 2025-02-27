using System;
using System.Collections.Generic;

namespace Dirassati_Backend.Domain.Models;

public partial class Parent
{
    public int ParentId { get; set; }

    public string UserId { get; set; } = null!;

    public string Occupation { get; set; } = null!;

    public int RelationshipToStudentId { get; set; }

    public virtual AspNetUser User { get; set; } = null!;
}
