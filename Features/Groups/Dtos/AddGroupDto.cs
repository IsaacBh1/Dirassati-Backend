using System;

namespace Dirassati_Backend.Features.Groups.Dtos;

using System.ComponentModel.DataAnnotations;

public class AddGroupDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string GroupName { get; set; } = null!;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "LevelId must be a positive integer.")]
    public int LevelId { get; set; }

    [Required]
    [Range(1, 100, ErrorMessage = "GroupCapacity must be between 1 and 100.")]
    public int GroupCapacity { get; set; }

    [Required]
    [MinLength(2, ErrorMessage = "At least two students must be provided.")]
    public List<Guid> StudentIds { get; set; } = null!;
}
