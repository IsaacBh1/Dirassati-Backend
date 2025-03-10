using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Data.Models;

public class SchoolType
{
    [Key]
    public int SchoolTypeId { get; set; }
    public string Name { get; set; } = null!;
}
