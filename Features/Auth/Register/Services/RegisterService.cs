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
public class RegisterService(
    UserManager<AppUser> userManager,
    AppDbContext dbContext,
    IMapper mapper,
    IPhotoUploadService photoUploadService,
    ILogger<RegisterService> logger)
{
    public async Task<Result<CreatedEmployeeDto, string>> Register(RegisterDto dto)
    {
        logger.LogInformation("Starting employee registration process for email: {Email}", dto.Employee.Email);

        var result = new Result<CreatedEmployeeDto, string>();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            logger.LogDebug("Creating new AppUser for registration");
            var user = new AppUser
            {
                FirstName = dto.Employee.FirstName,
                LastName = dto.Employee.LastName,
                Email = dto.Employee.Email,
                PhoneNumber = dto.Employee.PhoneNumber,
                UserName = dto.Employee.Email
            };

            logger.LogDebug("Attempting to create user with UserManager");
            var CreateResult = await userManager.CreateAsync(user, dto.Employee.Password);

            if (!CreateResult.Succeeded)
            {
                logger.LogWarning("User creation failed for email: {Email}. Errors: {Errors}",
                    dto.Employee.Email, string.Join(", ", CreateResult.Errors.Select(e => e.Description)));

                var errorBuilder = new StringBuilder();
                foreach (var error in CreateResult.Errors)
                {
                    errorBuilder.AppendLine(error.Description);
                }
                var errors = errorBuilder.ToString();
                return result.Failure(errors, 400);
            }

            logger.LogInformation("User created successfully for email: {Email}", dto.Employee.Email);

            var Specializations = new List<Specialization>();

            if (dto.School.SpecializationsId != null)
            {
                logger.LogDebug("Fetching specializations for school. Count: {Count}", dto.School.SpecializationsId.Count);
                Specializations = await dbContext.Specializations.Where(s => dto.School.SpecializationsId.Contains(s.SpecializationId)).ToListAsync();
                logger.LogDebug("Found {Count} specializations", Specializations.Count);
            }

            logger.LogDebug("Creating school entity");
            var school = mapper.Map<Data.Models.School>(dto.School);
            school.Specializations = Specializations;
            school.PhoneNumbers.Add(new PhoneNumber { Number = dto.School.PhoneNumber });

            logger.LogDebug("Creating employee entity");
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

            logger.LogDebug("Saving school and employee to database");
            await dbContext.Schools.AddAsync(school);
            var newEmployee = await dbContext.Employees.AddAsync(employee);
            await dbContext.SaveChangesAsync();

            logger.LogDebug("Committing transaction");
            await transaction.CommitAsync();

            logger.LogInformation("Employee registration completed successfully for email: {Email}, Employee ID: {EmployeeId}",
                dto.Employee.Email, newEmployee.Entity.EmployeeId);

            return result.Success(newEmployee.ToEmployeeResponceDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during employee registration for email: {Email}", dto.Employee.Email);
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<CreatedEmployeeDto, string>> RegisterWithImageUpload(ImprovedRegisterDto dto)
    {
        logger.LogInformation("Starting employee registration with image upload for email: {Email}", dto.Email);

        var result = new Result<CreatedEmployeeDto, string>();
        var registerDto = dto.ToRegisterDto();

        try
        {
            if (dto.SchoolLogo is not null)
            {
                logger.LogDebug("Uploading school logo. File size: {FileSize} bytes", dto.SchoolLogo.Length);
                var schoolLogoUploadResult = await photoUploadService.UploadPhotoAsync(dto.SchoolLogo);

                if (!schoolLogoUploadResult.IsSuccess)
                {
                    logger.LogWarning("School logo upload failed for email: {Email}. Errors: {Errors}",
                        dto.Email, schoolLogoUploadResult.Errors);
                    return result.Failure(schoolLogoUploadResult.Errors!, schoolLogoUploadResult.StatusCode);
                }

                logger.LogDebug("School logo uploaded successfully. URL: {LogoUrl}", schoolLogoUploadResult.Value?.Url);
                registerDto.School.LogoUrl = schoolLogoUploadResult.Value?.Url;
            }

            if (dto.ProfilePicture is null)
            {
                logger.LogDebug("No profile picture provided, proceeding with registration");
                return await Register(registerDto);
            }

            logger.LogDebug("Uploading profile picture. File size: {FileSize} bytes", dto.ProfilePicture.Length);
            var profilePictureUploadResult = await photoUploadService.UploadPhotoAsync(dto.ProfilePicture);

            if (!profilePictureUploadResult.IsSuccess)
            {
                logger.LogWarning("Profile picture upload failed for email: {Email}. Errors: {Errors}",
                    dto.Email, profilePictureUploadResult.Errors);
                return result.Failure(profilePictureUploadResult.Errors!, profilePictureUploadResult.StatusCode);
            }

            logger.LogDebug("Profile picture uploaded successfully. URL: {ProfilePictureUrl}", profilePictureUploadResult.Value?.Url);
            registerDto.Employee.ProfilePictureUrl = profilePictureUploadResult.Value?.Url;

            return await Register(registerDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during employee registration with image upload for email: {Email}", dto.Email);
            return result.Failure(ex.Message, 500);
        }
    }
}
