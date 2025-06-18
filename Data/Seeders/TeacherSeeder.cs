using Dirassati_Backend.Domain.Models;
using Dirassati_Backend.Features.Teachers.Dtos;
using Dirassati_Backend.Features.Teachers.Services;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Data.Seeders;

public static class TeacherSeeder
{
    public static void SeedContractType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContractType>()
        .HasData(
            new ContractType { ContractId = 1, Name = "Contrats Permanents" },
            new ContractType { ContractId = 2, Name = "Contrats à Durée Déterminée" },
            new ContractType { ContractId = 3, Name = "Contrats à Temps Partiel ou Horaire" },
            new ContractType { ContractId = 4, Name = "Stagiaire" },
            new ContractType { ContractId = 5, Name = "Consultant Pédagogique" },
            new ContractType { ContractId = 6, Name = "Bénévole" }
        );
    }
    public static async Task SeedTeachersAsync(Guid schoolId, TeacherServices teacherServices)
    {
        var random = new Random();

        // Algerian names for teachers
        var maleNames = new[] { "Ahmed", "Mohamed", "Abderrahim", "Karim", "Youcef", "Omar", "Ali", "Sofiane", "Hamid", "Farid", "Rachid", "Tarek", "Bilal", "Walid", "Samir", "Nabil", "Amine", "Hakim", "Riad", "Djamel" };
        var femaleNames = new[] { "Fatima", "Aicha", "Khadija", "Yamina", "Zohra", "Amina", "Malika", "Samira", "Naima", "Farida", "Zineb", "Leila", "Nadia", "Warda", "Houria", "Karima", "Souad", "Hafida", "Djamila", "Sabrina" };
        var lastNames = new[] { "Benali", "Boumediene", "Cherif", "Djebbar", "Ferhat", "Ghazi", "Hamdi", "Ikhlef", "Jazairi", "Karim", "Lamri", "Meziane", "Nouri", "Ouali", "Pasha", "Qadri", "Rahmi", "Saadi", "Tebboune", "Umar" };

        var teachers = new List<TeacherInfosDto>();
        var usedCombinations = new HashSet<string>();

        // Create 8-12 teachers per school
        var teacherCount = random.Next(8, 13);

        for (int i = 0; i < teacherCount; i++)
        {
            string firstName, lastName, email;
            int attempts = 0;

            // Keep trying until we get a unique combination
            do
            {
                var isFemale = random.Next(2) == 0;
                firstName = isFemale ?
                    femaleNames[random.Next(femaleNames.Length)] :
                    maleNames[random.Next(maleNames.Length)];
                lastName = lastNames[random.Next(lastNames.Length)];

                // Add school-specific identifier to ensure uniqueness across schools
                email = $"{firstName.ToLower()}.{lastName.ToLower()}.{schoolId.ToString()[..8]}@teacher.dz";
                attempts++;

                // Fallback: add number if still duplicate after many attempts
                if (attempts > 10)
                {
                    email = $"{firstName.ToLower()}.{lastName.ToLower()}.{i}@teacher.dz";
                    break;
                }
            }
            while (usedCombinations.Contains(email));

            usedCombinations.Add(email);

            teachers.Add(new TeacherInfosDto
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneNumber = GenerateAlgerianPhoneNumber(random),
                HireDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-random.Next(1, 15))),
                ContractTypeId = random.Next(1, 7), // Random contract type
                SchoolId = schoolId
            });
        }

        foreach (var teacher in teachers)
        {
            await teacherServices.RegisterTeacherAsync(teacher, schoolId.ToString(), true);
        }

        Console.WriteLine($"Successfully seeded {teachers.Count} teachers for school {schoolId}");
    }

    private static string GenerateAlgerianPhoneNumber(Random random)
    {
        var prefixes = new[] { "05", "06", "07" };
        var prefix = prefixes[random.Next(prefixes.Length)];
        var number = random.Next(10000000, 99999999);
        return $"+213{prefix}{number:D8}";
    }
}
