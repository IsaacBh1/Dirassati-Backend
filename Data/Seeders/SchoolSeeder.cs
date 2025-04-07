using Dirassati_Backend.Features.Auth.Register.Dtos;
using Dirassati_Backend.Features.Auth.SignUp;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Dirassati_Backend.Data.Seeders
{
    public class SchoolSeeder
    {


        public static async Task<string?> SeedAsync(RegisterService registerService, AppDbContext dbContext)
        {
            var registerDto = new RegisterDto
            {
                School = new SchoolDto
                {
                    SchoolName = "Greenwood High School",
                    SchoolTypeId = 3,
                    SchoolEmail = "info@greenwoodhigh.edu",
                    PhoneNumber = "+1234567890",
                    Address = new RegisterAddressDto
                    {
                        Street = "123 Elm Street",
                        City = "Springfield",
                        State = "Illinois",
                        PostalCode = "62704",
                        Country = "USA"
                    },
                    SpecializationsId = [
                        1,2,3,4,5
                    ]
                },
                Employee = new EmployeeDto
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "test123@test.com",
                    PhoneNumber = "+1987654321",
                    Password = "P@ssword123",
                    Permission = 1
                },
            };

            var school = await dbContext.Schools.FirstOrDefaultAsync(s => s.Email == registerDto.School.SchoolEmail);
            if (school != null)
                return school.SchoolId.ToString();
            var result = await registerService.Register(registerDto);
            if (result.IsSuccess && result.Value != null)
            {
                Console.WriteLine("Successfully seeded Greenwood High School and employee.");
                return result.Value.SchoolId;
            }
            else
            {
                Console.WriteLine("Failed to seed Greenwood High School and employee.");
                if (result.Errors != null)
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error: {error.Description}");
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {result.Errors}");
                }
                return null;
            }

        }


    }

}