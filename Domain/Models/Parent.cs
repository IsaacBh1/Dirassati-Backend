using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Domain.Models;

public partial class Parent
{
    [Key]
    public Guid ParentId { get; set; }

    public Guid UserId { get; set; } = Guid.Empty;

    public string Occupation { get; set; } = null!;

    public int RelationshipToStudentId { get; set; }

    public virtual AppUser User { get; set; } = null!;
    public RelationshipToStudent relationshipToStudent { get; set; } = null!;
}
