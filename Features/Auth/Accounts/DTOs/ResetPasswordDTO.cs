using System;
using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Features.Auth.Accounts.DTOs;

public class ResetPasswordDTO
{
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public string OldPassword { get; set; } = null!;
    [Required]
    public string NewPassword { get; set; } = null!;
}
