using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dirassati_Backend.Domain.Models;

public partial class Classroom
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ClassroomId { get; set; }

    public string ClassName { get; set; } = null!;

    public string SchoolId { get; set; } = null!;

    public virtual School School { get; set; } = null!;
}
