using System;
using Dirassati_Backend.Features.Auth.Register.Dtos;

namespace Dirassati_Backend.Features.Auth.SignUp;

public class RegisterDto
{
    public required SchoolDto School { get; set; }
    public required EmployeeDto Employee { get; set; }
}
