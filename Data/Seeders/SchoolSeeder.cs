using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Auth.Register.Dtos;
using Dirassati_Backend.Features.Auth.SignUp;
using Microsoft.AspNetCore.Identity;
using Persistence;

namespace Dirassati_Backend.Data.Seeders
{
    public class SchoolSeeder
    {


        public static async Task SeedAsync(RegisterService registerService)
        {
            var registerDto = new RegisterDto
            {
                School = new SchoolDto
                {
                    SchoolName = "Greenwood High School",
                    SchoolTypeId = 3,
                    SchoolEmail = "info@greenwoodhigh.edu",
                    PhoneNumber = "+1234567890",
                    Address = new AddressDto
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
                    Permission = 2
                },


            };

            try
            {
                var result = await registerService.Register(registerDto);

                if (result.IsSuccess)
                {
                    Console.WriteLine("Successfully seeded Greenwood High School and employee.");
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
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred during seeding: {ex.Message}");
            }
        }
    }
}