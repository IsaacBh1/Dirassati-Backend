using System.Text;
using AutoMapper;
using Dirassati_Backend.Common;
using Dirassati_Backend.Common.Services.PhotoUpload;
using Dirassati_Backend.Data;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Auth.Register.Dtos;
using Dirassati_Backend.Features.Auth.Register.Extensions;
using Dirassati_Backend.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Features.Auth.Register.Services;


[Tags("Employee Authentication")]

public class RegisterService(UserManager<AppUser> userManager, AppDbContext dbContext, IMapper mapper, IPhotoUploadService photoUploadService)
{
    public async Task<Result<CreatedEmployeeDto, string>> Register(RegisterDto dto)
    {
        var result = new Result<CreatedEmployeeDto, string>();
        using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            var user = new AppUser
            {
                FirstName = dto.Employee.FirstName,
                LastName = dto.Employee.LastName,
                Email = dto.Employee.Email,
                PhoneNumber = dto.Employee.PhoneNumber,
                UserName = dto.Employee.Email
            };

            var CreateResult = await userManager.CreateAsync(user, dto.Employee.Password);
            if (!CreateResult.Succeeded)
            {
                var errorBuilder = new StringBuilder();
                foreach (var error in CreateResult.Errors)
                {
                    errorBuilder.AppendLine(error.Description);
                }
                var errors = errorBuilder.ToString();
                return result.Failure(errors, 400);
            }
            var Specializations = new List<Specialization>();

            if (dto.School.SpecializationsId != null)
            {
                Specializations = await dbContext.Specializations.Where(s => dto.School.SpecializationsId.Contains(s.SpecializationId)).ToListAsync();
            }
            var school = mapper.Map<Data.Models.School>(dto.School);
            school.Specializations = Specializations;
            school.PhoneNumbers.Add(new PhoneNumber { Number = dto.School.PhoneNumber });

            var employee = new Employee
            {
                ContractType = "Permanent",
                HireDate = DateOnly.FromDateTime(DateTime.Now),
                IsActive = true,
                Position = "Admin",
                School = school,
                User = user,
                Permissions = dto.Employee.Permission,
                ProfilePictureUrl = dto.Employee.ProfilePictureUrl
            };

            await dbContext.Schools.AddAsync(school);
            var newEmployee = await dbContext.Employees.AddAsync(employee);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return result.Success(newEmployee.ToEmployeeResponceDto());

        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<CreatedEmployeeDto, string>> RegisterWithImageUpload(ImprovedRegisterDto dto)
    {
        var result = new Result<CreatedEmployeeDto, string>();
        var registerDto = dto.ToRegisterDto();
        try
        {
            if (dto.SchoolLogo is not null)
            {
                var schoolLogoUploadResult = await photoUploadService.UploadPhotoAsync(dto.SchoolLogo);
                if (!schoolLogoUploadResult.IsSuccess)
                {
                    return result.Failure(schoolLogoUploadResult.Errors!, schoolLogoUploadResult.StatusCode);
                }

                registerDto.School.LogoUrl = schoolLogoUploadResult.Value.Url;
            }

            if (dto.ProfilePicture is null) return await Register(registerDto);
            var profilePictureUploadResult = await photoUploadService.UploadPhotoAsync(dto.ProfilePicture);
            if (!profilePictureUploadResult.IsSuccess)
            {
                return result.Failure(profilePictureUploadResult.Errors!, profilePictureUploadResult.StatusCode);
            }

            registerDto.Employee.ProfilePictureUrl = profilePictureUploadResult.Value.Url;

            return await Register(registerDto);
        }
        catch (Exception e)
        {
            return result.Failure(e.Message, 500);
            throw;
        }
    }
}
