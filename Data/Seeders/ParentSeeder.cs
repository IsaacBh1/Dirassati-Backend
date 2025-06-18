using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Data;
using Dirassati_Backend.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Data.Seeders
{
    public static class ParentSeeder
    {
        public static async Task SeedParentsAndStudentsAsync(AppDbContext dbContext,
            List<(ParentData parent, List<StudentData> students)> parentsAndStudents, Guid schoolId,
            UserManager<AppUser> userManager)
        {
            var random = new Random();

            foreach (var (parentData, studentsData) in parentsAndStudents)
            {
                // Create AppUser for parent
                var parentUser = new AppUser
                {
                    UserName = parentData.Email,
                    Email = parentData.Email,
                    FirstName = parentData.FirstName,
                    LastName = parentData.LastName,
                    BirthDate = parentData.BirthDate,
                    PhoneNumber = parentData.PhoneNumber,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(parentUser, "Parent@123");
                if (!result.Succeeded)
                {
                    Console.WriteLine($"Failed to create user for parent {parentData.FirstName} {parentData.LastName}");
                    continue;
                }

                // Create Parent entity
                var parent = new Parent
                {
                    ParentId = Guid.NewGuid(),
                    UserId = parentUser.Id,
                    Occupation = parentData.Occupation,
                    NationalIdentityNumber = parentData.NationalIdentityNumber,
                    User = parentUser
                };

                dbContext.Parents.Add(parent);
                await dbContext.SaveChangesAsync();

                // Create students for this parent
                foreach (var studentData in studentsData)
                {
                    var student = new Student
                    {
                        StudentId = Guid.NewGuid(),
                        FirstName = studentData.FirstName,
                        LastName = studentData.LastName,
                        Address = studentData.Address,
                        BirthDate = studentData.BirthDate,
                        BirthPlace = studentData.BirthPlace,
                        SchoolId = schoolId,
                        StudentIdNumber = studentData.StudentIdNumber,
                        EmergencyContact = studentData.EmergencyContact,
                        SchoolLevelId = studentData.SchoolLevelId,
                        SpecializationId = studentData.SpecializationId,
                        ParentRelationshipToStudentTypeId = random.Next(1, 4), // Assuming 1-3 are valid relationship types
                        EnrollmentDate = studentData.EnrollmentDate,
                        ParentId = parent.ParentId,
                        IsActive = true
                    };

                    dbContext.Students.Add(student);
                }

                await dbContext.SaveChangesAsync();
            }

            Console.WriteLine($"Successfully seeded {parentsAndStudents.Count} families with their students for school {schoolId}");
        }
    }
}
