using Dirassati_Backend.Domain.Models;
using Dirassati_Backend.Features.Auth.Register.Dtos;

namespace Dirassati_Backend.Features.Auth.Register.Extensions;

public static class RegisterExtensions
{
    public static CreatedEmployeeDto ToEmployeeResponceDto(this Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Employee> source) => new CreatedEmployeeDto
    {
        Email = source.Entity.User.Email!,
        FirstName = source.Entity.User.FirstName,
        LastName = source.Entity.User.LastName,
        EmployeeId = source.Entity.EmployeeId,
        PhoneNumber = source.Entity.User.PhoneNumber!,
        Permissions = source.Entity.Permissions,
    };
}
