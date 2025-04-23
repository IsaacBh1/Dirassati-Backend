// filepath: /mnt/60D2B11CD2B0F77E/projects/Dirasati/Dirassati-Backend/Features/Students/DTOs/UpdateStudentDto.cs
using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Features.Students.DTOs;

/// <summary>
/// Data transfer object for updating student information
/// </summary>
public class UpdateStudentDto
{
    /// <summary>
    /// Student's first name
    /// </summary>
    [Required]
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Student's last name
    /// </summary>
    [Required]
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Student's address
    /// </summary>
    [Required]
    public string Address { get; set; } = null!;

    /// <summary>
    /// Student's birth date
    /// </summary>
    [Required]
    public DateOnly BirthDate { get; set; }

    /// <summary>
    /// Student's birth place
    /// </summary>
    [Required]
    public string BirthPlace { get; set; } = null!;

    /// <summary>
    /// Student's emergency contact
    /// </summary>
    [Required]
    public string EmergencyContact { get; set; } = null!;

    /// <summary>
    /// ID of the school level
    /// </summary>
    [Required]
    public int SchoolLevelId { get; set; }

    /// <summary>
    /// ID of the specialization (if applicable)
    /// </summary>
    public int? SpecializationId { get; set; }

    /// <summary>
    /// Whether the student is active
    /// </summary>
    [Required]
    public bool IsActive { get; set; }
}