using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dirassati_Backend.Data.Models;

public partial class Address
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AdresseId { get; set; }

    public string City { get; set; } = null!;

    public string? State { get; set; }

    public string Country { get; set; } = null!;
    public string Street { get; set; } = null!;
    public string? PostalCode { get; set; }
    public virtual School? School { get; set; }

}
