using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dirassati_Backend.Data.Models;

public partial class PhoneNumber
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

    public int PhoneNumberId { get; set; }

    public string Number { get; set; } = null!;

    public Guid SchoolId { get; set; } = Guid.Empty!;

    public virtual School School { get; set; } = null!;
}
