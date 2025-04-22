using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dirassati_Backend.Data.Models;

namespace Dirassati_Backend.Domain.Models;

public partial class ContractType
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

    public int ContractId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Teacher> Teachers { get; set; } = [];
}
