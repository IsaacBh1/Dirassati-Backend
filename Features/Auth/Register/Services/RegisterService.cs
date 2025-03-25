using Dirassati_Backend.Common;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Auth.Register.Dtos;
using Dirassati_Backend.Features.Auth.Register.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;
using SQLitePCL;

namespace Dirassati_Backend.Features.Auth.SignUp;


[Tags("Employee Authentication")]

public class RegisterService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _dbContext;

    public RegisterService(UserManager<AppUser> userManager, AppDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }
    public async Task<Result<CreatedEmployeeDto, IEnumerable<IdentityError>>> Register(RegisterDto dto)
    {
        var result = new Result<CreatedEmployeeDto, IEnumerable<IdentityError>>();
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
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

            var CreateResult = await _userManager.CreateAsync(user, dto.Employee.Password);
            if (!CreateResult.Succeeded)
            {
                return result.Failure(CreateResult.Errors, 400);
            }
            var Specializations = new List<Specialization>();

            if (dto.School.SpecializationsId != null)
            {
                Specializations = await _dbContext.Specializations.Where(s => dto.School.SpecializationsId.Contains(s.SpecializationId)).ToListAsync();
            }
            var school = new Data.Models.School
            {
                Name = dto.School.SchoolName,
                SchoolTypeId = dto.School.SchoolTypeId,
                Address = new Address
                {
                    City = dto.School.Address.City,
                    Country = dto.School.Address.Country,
                    State = dto.School.Address.State,
                    Street = dto.School.Address.Street
                },
                Email = dto.School.SchoolEmail,
                Logo = string.Empty,
                WebsiteUrl = string.Empty,
                SchoolConfig = string.Empty,
                Specializations = Specializations
            };


            var employee = new Employee
            {
                ContractType = "Permanent",
                HireDate = DateOnly.FromDateTime(DateTime.Now),
                IsActive = true,
                Position = "Admin",
                School = school,
                User = user,
                Permissions = dto.Employee.Permission
            };

            await _dbContext.Schools.AddAsync(school);
            var newEmployee = await _dbContext.Employees.AddAsync(employee);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return result.Success(newEmployee.ToEmployeeResponceDto());

        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
