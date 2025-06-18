using Dirassati_Backend.Data.Enums;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Data.Seeders
{
    public static class ClassroomSeeder
    {
        public static async Task SeedClassroomsAndGroupsAsync(AppDbContext dbContext, Guid schoolId, SchoolTypeEnum schoolType, Random random)
        {
            // Get school levels for this school type
            var schoolLevels = await dbContext.SchoolLevels
                .Where(sl => sl.SchoolTypeId == (int)schoolType)
                .ToListAsync();

            foreach (var schoolLevel in schoolLevels)
            {
                if (schoolType == SchoolTypeEnum.Lycee)
                {
                    // For Lycee, create classrooms for each specialization
                    var specializations = await dbContext.Specializations
                        .Where(s => s.Schools.Any(school => school.SchoolId == schoolId))
                        .ToListAsync();

                    foreach (var specialization in specializations)
                    {
                        await CreateClassroomAndGroup(dbContext, schoolId, schoolLevel, specialization, random);
                    }
                }
                else
                {
                    // For Primaire and Moyenne, create 1-2 classrooms per level without specialization
                    var classroomCount = random.Next(1, 3); // 1-2 classrooms per level
                    for (int i = 0; i < classroomCount; i++)
                    {
                        await CreateClassroomAndGroup(dbContext, schoolId, schoolLevel, null, random, i + 1);
                    }
                }
            }
        }

        private static async Task CreateClassroomAndGroup(AppDbContext dbContext, Guid schoolId,
            SchoolLevel schoolLevel, Specialization? specialization, Random random, int? sectionNumber = null)
        {            // Generate classroom name
            var levelName = GetLevelName(schoolLevel.LevelId);
            string className;
            if (specialization != null)
            {
                className = $"{levelName} - {specialization.Name}";
            }
            else if (sectionNumber.HasValue)
            {
                className = $"{levelName} - Section {sectionNumber}";
            }
            else
            {
                className = levelName;
            }

            // Create classroom
            var classroom = new Classroom
            {
                ClassroomId = Guid.NewGuid(),
                ClassName = className,
                SchoolId = schoolId,
                SchoolLevelId = schoolLevel.LevelId,
                SpecializationId = specialization?.SpecializationId
            };

            dbContext.Classrooms.Add(classroom);
            await dbContext.SaveChangesAsync();

            // Create group for this classroom
            var group = new Group
            {
                GroupId = Guid.NewGuid(),
                GroupName = className,
                GroupCapacity = random.Next(25, 36), // 25-35 students capacity
                SchoolId = schoolId,
                ClassroomId = classroom.ClassroomId
            };

            dbContext.Groups.Add(group);
            await dbContext.SaveChangesAsync();

            // Assign students to this group based on level and specialization
            await AssignStudentsToGroup(dbContext, schoolId, group, schoolLevel.LevelId, specialization?.SpecializationId);
        }

        private static async Task AssignStudentsToGroup(AppDbContext dbContext, Guid schoolId, Group group,
            int schoolLevelId, int? specializationId)
        {
            var studentsQuery = dbContext.Students
                .Where(s => s.SchoolId == schoolId && s.SchoolLevelId == schoolLevelId && s.GroupId == null);

            if (specializationId.HasValue)
            {
                studentsQuery = studentsQuery.Where(s => s.SpecializationId == specializationId.Value);
            }
            else
            {
                // For non-lycee levels, get students without specialization
                studentsQuery = studentsQuery.Where(s => s.SpecializationId == null);
            }

            var studentsToAssign = await studentsQuery
                .Take(group.GroupCapacity)
                .ToListAsync();

            foreach (var student in studentsToAssign)
            {
                student.GroupId = group.GroupId;
            }

            await dbContext.SaveChangesAsync();

            Console.WriteLine($"Assigned {studentsToAssign.Count} students to group {group.GroupName}");
        }

        private static string GetLevelName(int levelId)
        {
            return levelId switch
            {
                1 => "1AP",    // Primaire 1er
                2 => "2AP",    // Primaire 2eme
                3 => "3AP",    // Primaire 3eme
                4 => "4AP",    // Primaire 4eme
                5 => "5AP",    // Primaire 5eme
                6 => "1AM",    // Moyenne 1er
                7 => "2AM",    // Moyenne 2eme
                8 => "3AM",    // Moyenne 3eme
                9 => "4AM",    // Moyenne 4eme
                10 => "1AS",   // Lycee 1er
                11 => "2AS",   // Lycee 2eme
                12 => "3AS",   // Lycee 3eme
                _ => $"Level {levelId}"
            };
        }
    }
}
