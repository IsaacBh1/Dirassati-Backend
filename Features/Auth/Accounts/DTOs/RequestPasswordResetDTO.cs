using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Features.Auth.Accounts.DTOs;

public class RequestPasswordResetDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(256, ErrorMessage = "Email must not exceed 256 characters")]
    public string Email { get; set; } = "";
}
