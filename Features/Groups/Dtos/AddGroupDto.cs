namespace Dirassati_Backend.Features.Groups.Dtos;

using System.ComponentModel.DataAnnotations;

public class AddGroupDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string GroupName { get; set; } = null!;

    public Guid ClassroomId { get; set; }
    public List<Guid>? StudentIds { get; set; } = null!;
}
