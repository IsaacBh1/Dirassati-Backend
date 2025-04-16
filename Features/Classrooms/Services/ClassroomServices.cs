using Dirassati_Backend.Common;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Classrooms.Dtos;
using Dirassati_Backend.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Dirassati_Backend.Features.Classrooms.Services;

public class ClassroomServices(AppDbContext context) : IClassroomServices
{
    public async Task<Result<ClassroomDto, string>> AddClassroomAsync(AddClassroomDto addClassroomDto, string schoolId)
    {
        var result = new Result<ClassroomDto, string>();
    
        if (!Guid.TryParse(schoolId, out var parsedSchoolId))
        {
            return result.Failure("Invalid school ID", (int)HttpStatusCode.BadRequest);
        }

        // Check if school exists
        var school = await context.Schools.FindAsync(parsedSchoolId);
        if (school == null)
        {
            return result.Failure("School not found", (int)HttpStatusCode.NotFound);
        }

        // Check if school level exists
        var level = await context.SchoolLevels.FindAsync(addClassroomDto.SchoolLevelId);
        if (level == null)
        {
            return result.Failure("School level not found", (int)HttpStatusCode.NotFound);
        }

        // Check if the classroom name is already taken in this school
        var existingClassroom = await context.Classrooms
            .FirstOrDefaultAsync(c => c.SchoolId == parsedSchoolId &&
                                      c.ClassName.ToLower() == addClassroomDto.ClassName.ToLower());
        if (existingClassroom != null)
        {
            return result.Failure("A classroom with this name already exists",
                (int)HttpStatusCode.BadRequest);
        }

        var classroom = new Classroom
        {
            ClassroomId = Guid.NewGuid(),
            ClassName = addClassroomDto.ClassName,
            SchoolId = parsedSchoolId,
            SchoolLevelId = addClassroomDto.SchoolLevelId
        };

        context.Classrooms.Add(classroom);
        await context.SaveChangesAsync();

        var responseDto = new ClassroomDto
        {
            ClassroomId = classroom.ClassroomId,
            ClassName = classroom.ClassName,
            SchoolLevelId = classroom.SchoolLevelId,
            LevelName = $"Year {level.LevelYear}"
        };

        return result.Success(responseDto, (int)HttpStatusCode.Created);
    }
    public async Task<Result<List<ClassroomDto>, string>> GetAllClassroomsBySchoolAsync(string schoolId)
    {
        var result = new Result<List<ClassroomDto>, string>();
        if (!Guid.TryParse(schoolId, out Guid parsedSchoolId))
        {
            return result.Failure("Invalid school ID", (int)HttpStatusCode.BadRequest);
        }

        var classrooms = await context.Classrooms
            .Where(c => c.SchoolId == parsedSchoolId)
            .Include(c => c.SchoolLevel)
            .Select(c => new ClassroomDto
            {
                ClassroomId = c.ClassroomId,
                ClassName = c.ClassName,
                SchoolLevelId = c.SchoolLevelId,
                LevelName = $"Year {c.SchoolLevel.LevelYear}"
            })
            .ToListAsync();

        return result.Success(classrooms);
    }
    public async Task<Result<List<ClassroomDto>, string>> GetClassroomsBySchoolLevelAsync(int schoolLevelId, string schoolId)
    {
        var result = new Result<List<ClassroomDto>, string>();
        if (!Guid.TryParse(schoolId, out Guid parsedSchoolId))
        {
            return result.Failure("Invalid school ID", (int)HttpStatusCode.BadRequest);
        }

        var classrooms = await context.Classrooms
            .Where(c => c.SchoolId == parsedSchoolId && c.SchoolLevelId == schoolLevelId)
            .Include(c => c.SchoolLevel)
            .Select(c => new ClassroomDto
            {
                ClassroomId = c.ClassroomId,
                ClassName = c.ClassName,
                SchoolLevelId = c.SchoolLevelId,
                LevelName = $"Year {c.SchoolLevel.LevelYear}"
            })
            .ToListAsync();

        return result.Success(classrooms);
    }
}