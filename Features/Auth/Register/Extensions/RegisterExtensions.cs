using Dirassati_Backend.Common.Dtos;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Auth.Register.Dtos;

namespace Dirassati_Backend.Features.Auth.Register.Extensions;

public static class RegisterExtensions
{
    public static CreatedEmployeeDto ToEmployeeResponceDto(this Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Employee> source) => new()
    {
        Email = source.Entity.User.Email!,
        FirstName = source.Entity.User.FirstName,
        LastName = source.Entity.User.LastName,
        EmployeeId = source.Entity.EmployeeId,
        PhoneNumber = source.Entity.User.PhoneNumber!,
        Permissions = source.Entity.Permissions,
        SchoolId = source.Entity.SchoolId.ToString(),
        ProfilePicture = source.Entity.ProfilePictureUrl
    };
    public static RegisterDto ToRegisterDto(this ImprovedRegisterDto source) => new()
    {
        Employee = new EmployeeDto
        {
            FirstName = source.FirstName,
            LastName = source.LastName,
            Email = source.Email,
            PhoneNumber = source.PhoneNumber,
            Password = source.Password,
            Permission = source.Permission,
            ProfilePictureUrl = ""
        },
        School = new SchoolDto
        {
            Name = source.SchoolName,
            SchoolTypeId = source.SchoolTypeId,
            Email = source.SchoolEmail,
            PhoneNumber = source.SchoolPhoneNumber,
            BankCode = source.BankCode,
            SpecializationsId = source.SpecializationsId,
            LogoUrl = "",
            WebsiteUrl = source.WebsiteUrl,
            AcademicYear = new AcademicYearDto
            {
                StartDate = source.StartDate,
                EndDate = source.EndDate
            },
            Address = new AddressDto
            {
                PostalCode = source.PostalCode,
                City = source.City,
                Country = source.Country,
                State = source.State,
                Street = source.Street

            }
        }
    };
}
