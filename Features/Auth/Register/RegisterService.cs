using System;
using Dirassati_Backend.Domain.Models;
using Dirassati_Backend.Features.Auth.Register.Dtos;
using Dirassati_Backend.Features.Auth.Register.Extensions;
using Microsoft.AspNetCore.Identity;
using Persistence;

namespace Dirassati_Backend.Features.Auth.SignUp;


[Tags("Employee Authentication")]

public class RegisterService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _dbContext;

    public RegisterService(UserManager<AppUser> userManager, AppDbContext dbContext)
    {
        this._userManager = userManager;
        this._dbContext = dbContext;
    }
    public async Task<CreatedEmployeeDto?> Register(RegisterDto dto)
    {
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

            var result = await _userManager.CreateAsync(user, dto.Employee.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Error: {error.Code} - {error.Description}");
                }
                return null;
            }

            var address = new Address
            {
                City = dto.School.Address.City,
                Country = dto.School.Address.Country,
                State = dto.School.Address.State,
                Street = dto.School.Address.Street
            };

            var school = new School
            {
                Name = dto.School.SchoolName,
                SchoolType = dto.School.SchoolType,
                Address = address,
                Email = dto.School.SchoolEmail,
                Logo = string.Empty,
                WebsiteUrl = string.Empty,
                SchoolConfig = string.Empty
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

            return newEmployee.ToEmployeeResponceDto();

        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
