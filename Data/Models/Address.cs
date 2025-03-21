using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dirassati_Backend.Domain.Models;

public partial class Address
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AdresseId { get; set; }

    public string? Street { get; set; } 
    public string? City { get; set; } 
    public string? State { get; set; } 
    public string? PostalCode { get; set; } 
    public string? Country { get; set; } 


}
