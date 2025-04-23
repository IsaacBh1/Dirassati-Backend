using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Features.Groups.Dtos;

public class UpdateGroupDto
{
    [StringLength(100, MinimumLength = 2)]
    public string? GroupName { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Group capacity must be greater than 0")]
    public int? GroupCapacity { get; set; }

    public List<Guid>? StudentIds { get; set; }
}