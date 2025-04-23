using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Features.Groups.Dtos;

public class AssignStudentToGroupDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid GroupId { get; set; }
}