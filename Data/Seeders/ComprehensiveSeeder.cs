using System.Globalization;
using Dirassati_Backend.Common.Dtos;
using Dirassati_Backend.Data.Enums;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Auth.Register.Dtos;
using Dirassati_Backend.Features.Auth.Register.Services;
using Dirassati_Backend.Features.Teachers.Services;
using Dirassati_Backend.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Dirassati_Backend.Data;

namespace Dirassati_Backend.Data.Seeders
{
    public static class ComprehensiveSeeder
    {
        private static readonly string[] AlgerianStates = [
            "Algiers", "Oran", "Constantine", "Batna", "Bejaia", "Tlemcen",
            "Setif", "Annaba", "Blida", "Djelfa", "Jijel", "Ouargla",
            "Mascara", "Biskra", "Skikda", "Tiaret", "Ghardaia", "El Oued"
        ];

        private static readonly string[] AlgerianMaleNames = [
            "Ahmed", "Mohamed", "Abderrahim", "Karim", "Youcef", "Omar", "Ali",
            "Sofiane", "Hamid", "Farid", "Rachid", "Tarek", "Bilal", "Walid",
            "Samir", "Nabil", "Amine", "Hakim", "Riad", "Djamel", "Mounir",
            "Khaled", "Noureddine", "Abdelkader", "Mustapha"
        ];

        private static readonly string[] AlgerianFemaleNames = [
            "Fatima", "Aicha", "Khadija", "Yamina", "Zohra", "Amina", "Malika",
            "Samira", "Naima", "Farida", "Zineb", "Leila", "Nadia", "Warda",
            "Houria", "Karima", "Souad", "Hafida", "Djamila", "Sabrina",
            "Assia", "Meriem", "Siham", "Soraya", "Imane"
        ];

        private static readonly string[] AlgerianLastNames = [
            "Benali", "Boumediene", "Cherif", "Djebbar", "Ferhat", "Ghazi",
            "Hamdi", "Ikhlef", "Jazairi", "Karim", "Lamri", "Meziane",
            "Nouri", "Ouali", "Pasha", "Qadri", "Rahmi", "Saadi",
            "Tebboune", "Umar", "Vali", "Wahab", "Xerri", "Yacoub", "Zahra"
        ];

        private static readonly string[] Occupations = [
            "Engineer", "Doctor", "Teacher", "Lawyer", "Businessman", "Accountant",
            "Pharmacist", "Architect", "Civil Servant", "Shop Owner", "Mechanic",
            "Driver", "Nurse", "Police Officer", "Farmer", "Electrician"
        ];

        private static readonly Dictionary<SchoolTypeEnum, List<string>> SchoolNames = new()
        {
            [SchoolTypeEnum.Primaire] = [
                "El Nour Primary School", "Al Amal Primary School", "Tarek Ibn Ziad Primary School",
                "Ibn Sina Primary School"
            ],
            [SchoolTypeEnum.Moyenne] = [
                "Mohamed Boudiaf Middle School", "Emir Abdelkader Middle School",
                "Ahmed Zabana Middle School"
            ],
            [SchoolTypeEnum.Lycee] = [
                "Houari Boumediene High School", "Frantz Fanon High School",
                "Abane Ramdane High School"
            ]
        };

        public static async Task SeedAllSchoolsAsync(RegisterService registerService, TeacherServices teacherServices,
            AppDbContext dbContext, UserManager<AppUser> userManager)
        {
            var random = new Random();
            var seededSchoolIds = new List<string>();

            // Seed different types of schools
            foreach (var schoolType in Enum.GetValues<SchoolTypeEnum>())
            {
                var schoolNamesForType = SchoolNames[schoolType];

                foreach (var schoolName in schoolNamesForType)
                {
                    var schoolId = await SeedSchoolAsync(registerService, dbContext, schoolType, schoolName, random);
                    if (schoolId != null)
                    {
                        seededSchoolIds.Add(schoolId);

                        // Seed teachers for this school
                        await TeacherSeeder.SeedTeachersAsync(Guid.Parse(schoolId), teacherServices);
                        // Seed students and parents for this school
                        await SeedStudentsAndParentsAsync(dbContext, Guid.Parse(schoolId), userManager, random);

                        // Seed classrooms and groups
                        await SeedClassroomsAndGroupsAsync(dbContext, Guid.Parse(schoolId), schoolType, random);
                    }
                }
            }

            Console.WriteLine($"Successfully seeded {seededSchoolIds.Count} schools with comprehensive data.");
        }

        private static async Task<string?> SeedSchoolAsync(RegisterService registerService, AppDbContext dbContext,
            SchoolTypeEnum schoolType, string schoolName, Random random)
        {            // Check if school already exists
            var existingSchool = await dbContext.Schools.FirstOrDefaultAsync(s => s.Name == schoolName);
            if (existingSchool != null)
                return existingSchool.SchoolId.ToString();

            var state = AlgerianStates[random.Next(AlgerianStates.Length)];
            var adminFirstName = AlgerianMaleNames[random.Next(AlgerianMaleNames.Length)];
            var adminLastName = AlgerianLastNames[random.Next(AlgerianLastNames.Length)];
            var schoolDomain = schoolName.Replace(" ", "").ToLower();
            var uniqueId = Guid.NewGuid().ToString()[..6]; // Short unique identifier

            // Generate appropriate specializations based on school type
            var specializationIds = GetSpecializationsForSchoolType(schoolType);

            var registerDto = new RegisterDto
            {
                School = new SchoolDto
                {
                    Name = schoolName,
                    SchoolTypeId = (int)schoolType,
                    Email = $"{adminFirstName.ToLower()}.{adminLastName.ToLower()}.{uniqueId}@{schoolDomain}.dz",
                    PhoneNumber = GenerateAlgerianPhoneNumber(random),
                    Address = new AddressDto
                    {
                        Street = $"{random.Next(1, 999)} Rue {AlgerianLastNames[random.Next(AlgerianLastNames.Length)]}",
                        City = state,
                        State = state,
                        PostalCode = random.Next(10000, 99999).ToString(),
                        Country = "Algeria"
                    },
                    SpecializationsId = specializationIds,
                    LogoUrl = $"https://logo.example.com/{schoolName.Replace(" ", "").ToLower()}.png",
                    WebsiteUrl = $"https://www.{schoolName.Replace(" ", "").ToLower()}.edu.dz",
                    AcademicYear = new AcademicYearDto
                    {
                        StartDate = DateOnly.Parse($"2024-{random.Next(9, 11)}-{random.Next(1, 15)}", CultureInfo.InvariantCulture),
                        EndDate = DateOnly.Parse($"2025-{random.Next(6, 8)}-{random.Next(15, 31)}", CultureInfo.InvariantCulture)
                    },
                    BillAmount = schoolType switch
                    {
                        SchoolTypeEnum.Primaire => random.Next(1500, 2500),
                        SchoolTypeEnum.Moyenne => random.Next(2000, 3500),
                        SchoolTypeEnum.Lycee => random.Next(3000, 5000),
                        _ => 2500
                    }
                },
                Employee = new EmployeeDto
                {
                    FirstName = adminFirstName,
                    LastName = adminLastName,
                    Email = $"{adminFirstName.ToLower()}.{adminLastName.ToLower()}.{uniqueId}@{schoolDomain}.dz",
                    PhoneNumber = GenerateAlgerianPhoneNumber(random),
                    Password = "Admin@123",
                    Permission = 1
                }
            };

            var result = await registerService.Register(registerDto);
            if (result.IsSuccess && result.Value != null)
            {
                Console.WriteLine($"Successfully seeded {schoolName}.");
                return result.Value.SchoolId;
            }
            else
            {
                Console.WriteLine($"Failed to seed {schoolName}.");
                return null;
            }
        }

        private static List<int> GetSpecializationsForSchoolType(SchoolTypeEnum schoolType)
        {
            return schoolType switch
            {
                SchoolTypeEnum.Primaire => [], // No specializations for primary
                SchoolTypeEnum.Moyenne => [], // No specializations for middle school
                SchoolTypeEnum.Lycee => [1, 2, 3, 4, 5, 6, 7, 8, 9, 10], // All specializations for high school
                _ => []
            };
        }

        private static async Task SeedStudentsAndParentsAsync(AppDbContext dbContext, Guid schoolId, UserManager<AppUser> userManager, Random random)
        {
            var school = await dbContext.Schools
                .Include(s => s.SchoolType)
                .FirstOrDefaultAsync(s => s.SchoolId == schoolId);

            if (school == null) return;

            var parentsAndStudents = new List<(ParentData parent, List<StudentData> students)>();

            // Create 15-20 families with 1-3 children each
            var familyCount = random.Next(15, 21);

            for (int i = 0; i < familyCount; i++)
            {
                var parent = GenerateParentData(random);
                var childrenCount = random.Next(1, 4); // 1-3 children
                var students = new List<StudentData>();

                for (int j = 0; j < childrenCount; j++)
                {
                    var student = GenerateStudentData(school, parent, random);
                    students.Add(student);
                }

                parentsAndStudents.Add((parent, students));
            }            // Now create the entities in the database
            await ParentSeeder.SeedParentsAndStudentsAsync(dbContext, parentsAndStudents, schoolId, userManager);
        }
        private static ParentData GenerateParentData(Random random)
        {
            var isMotherPrimary = random.Next(2) == 0;
            var fatherFirstName = AlgerianMaleNames[random.Next(AlgerianMaleNames.Length)];
            var motherFirstName = AlgerianFemaleNames[random.Next(AlgerianFemaleNames.Length)];
            var lastName = AlgerianLastNames[random.Next(AlgerianLastNames.Length)];
            var uniqueId = Guid.NewGuid().ToString()[..8]; // Add unique identifier

            return new ParentData
            {
                FirstName = isMotherPrimary ? motherFirstName : fatherFirstName,
                LastName = lastName,
                Email = $"{(isMotherPrimary ? motherFirstName : fatherFirstName).ToLower()}.{lastName.ToLower()}.{uniqueId}@email.dz",
                PhoneNumber = GenerateAlgerianPhoneNumber(random),
                Occupation = Occupations[random.Next(Occupations.Length)],
                NationalIdentityNumber = GenerateNationalId(random),
                BirthDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-random.Next(35, 55))),
                Address = $"{random.Next(1, 200)} Rue {AlgerianLastNames[random.Next(AlgerianLastNames.Length)]}, {AlgerianStates[random.Next(AlgerianStates.Length)]}"
            };
        }

        private static StudentData GenerateStudentData(School school, ParentData parent, Random random)
        {
            var isGirl = random.Next(2) == 0;
            var firstName = isGirl ?
                AlgerianFemaleNames[random.Next(AlgerianFemaleNames.Length)] :
                AlgerianMaleNames[random.Next(AlgerianMaleNames.Length)];

            // Generate age appropriate for school type
            var (minAge, maxAge) = school.SchoolType.SchoolTypeId switch
            {
                1 => (6, 11),  // Primaire
                2 => (11, 15), // Moyenne
                3 => (15, 18), // Lycee
                _ => (6, 18)
            };

            var age = random.Next(minAge, maxAge + 1);
            var birthDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-age));

            return new StudentData
            {
                FirstName = firstName,
                LastName = parent.LastName,
                BirthDate = birthDate,
                BirthPlace = AlgerianStates[random.Next(AlgerianStates.Length)],
                Address = parent.Address,
                EmergencyContact = parent.PhoneNumber,
                StudentIdNumber = $"STU{random.Next(100000, 999999)}",
                EnrollmentDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-random.Next(1, 24))),
                SchoolLevelId = GetSchoolLevelForAge(age, school.SchoolType.SchoolTypeId),
                SpecializationId = school.SchoolType.SchoolTypeId == 3 ? random.Next(1, 11) : null // Only for Lycee
            };
        }

        private static int GetSchoolLevelForAge(int age, int schoolTypeId)
        {
            return schoolTypeId switch
            {
                1 => age switch // Primaire
                {
                    6 => 1,  // Primaire1er
                    7 => 2,  // Primaire2eme
                    8 => 3,  // Primaire3eme
                    9 => 4,  // Primaire4eme
                    _ => 5   // Primaire5eme
                },
                2 => age switch // Moyenne
                {
                    11 => 6, // Moyenne1er
                    12 => 7, // Moyenne2eme
                    13 => 8, // Moyenne3eme
                    _ => 9   // Moyenne4eme
                },
                3 => age switch // Lycee
                {
                    15 => 10, // Lycee1er
                    16 => 11, // Lycee2eme
                    _ => 12   // Lycee3eme
                },
                _ => 1
            };
        }

        private static async Task SeedClassroomsAndGroupsAsync(AppDbContext dbContext, Guid schoolId, SchoolTypeEnum schoolType, Random random)
        {
            await ClassroomSeeder.SeedClassroomsAndGroupsAsync(dbContext, schoolId, schoolType, random);
        }

        private static string GenerateAlgerianPhoneNumber(Random random)
        {
            var prefixes = new[] { "05", "06", "07" };
            var prefix = prefixes[random.Next(prefixes.Length)];
            var number = random.Next(10000000, 99999999);
            return $"+213{prefix}{number:D8}";
        }

        private static string GenerateNationalId(Random random)
        {
            return $"{random.Next(100000000, 999999999):D9}{random.Next(10, 99):D2}";
        }
    }

    // Data transfer classes for internal use
    public class ParentData
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Occupation { get; set; } = string.Empty;
        public string NationalIdentityNumber { get; set; } = string.Empty;
        public DateOnly BirthDate { get; set; }
        public string Address { get; set; } = string.Empty;
    }

    public class StudentData
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateOnly BirthDate { get; set; }
        public string BirthPlace { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string EmergencyContact { get; set; } = string.Empty;
        public string StudentIdNumber { get; set; } = string.Empty;
        public DateOnly EnrollmentDate { get; set; }
        public int SchoolLevelId { get; set; }
        public int? SpecializationId { get; set; }
    }
}
