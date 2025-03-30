using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Features.Auth.Accounts.DTOs;

public class ChangePasswordDto
{
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W)[A-Za-z\d\W]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character")]
    public string OldPassword { get; set; } = "";

    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W)[A-Za-z\d\W]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character")]
    public string NewPassword { get; set; } = "";
}