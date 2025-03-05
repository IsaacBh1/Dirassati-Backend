using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dirassati_Backend.Data.Models;

public class SchoolType
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SchoolTypeId { get; set; }
    public string Name { get; set; } = null!;
}
