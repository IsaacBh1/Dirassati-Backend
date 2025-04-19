using Dirassati_Backend.Common;
using Dirassati_Backend.Common.Dtos;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Groups.Dtos;
using Dirassati_Backend.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Dirassati_Backend.Features.Groups.Services
{
    public class GroupServices(AppDbContext context, ILogger<GroupServices> logger) : IGroupServices
    {
        public async Task<Result<GroupDto, string>> AddGroupAsync(AddGroupDto addGroupDto, string schoolId)
        {
            var result = new Result<GroupDto, string>();

            try
            {
                logger.LogInformation("Adding new group with name: {GroupName}", addGroupDto.GroupName);

                if (!Guid.TryParse(schoolId, out var parsedSchoolId))
                {
                    logger.LogWarning("Invalid school ID provided: {SchoolId}", schoolId);
                    return result.Failure("Invalid school ID", (int)HttpStatusCode.BadRequest);
                }

                // Check if school exists
                var schoolExists = await context.Schools.AnyAsync(s => s.SchoolId == parsedSchoolId);
                if (!schoolExists)
                {
                    logger.LogWarning("School with ID {SchoolId} not found", parsedSchoolId);
                    return result.Failure("School not found", (int)HttpStatusCode.NotFound);
                }

                // Check if classroom exists
                var classroom = await context.Classrooms
                    .Include(c => c.SchoolLevel).ThenInclude(schoolLevel => schoolLevel.SchoolType)
                    .FirstOrDefaultAsync(c => c.ClassroomId == addGroupDto.ClassroomId);

                if (classroom == null)
                {
                    logger.LogWarning("Classroom with ID {ClassroomId} not found", addGroupDto.ClassroomId);
                    return result.Failure("Classroom not found", (int)HttpStatusCode.NotFound);
                }

                // Check if group name already exists in the school
                var groupNameExists = await context.Groups
                    .AnyAsync(g => g.SchoolId == parsedSchoolId && 
                                  g.GroupName.ToLower() == addGroupDto.GroupName.ToLower());

                if (groupNameExists)
                {
                    logger.LogWarning("Group with name {GroupName} already exists in school {SchoolId}",
                        addGroupDto.GroupName, parsedSchoolId);
                    return result.Failure("A group with this name already exists in your school",
                        (int)HttpStatusCode.BadRequest);
                }

                // Create the group
                var group = new Group
                {
                    GroupId = Guid.NewGuid(),
                    GroupName = addGroupDto.GroupName,
                    SchoolId = parsedSchoolId,
                    ClassroomId = classroom.ClassroomId,
                    GroupCapacity = 30 // Default capacity, can be adjusted as needed
                };

                context.Groups.Add(group);

                // Validate and add students if provided
                if (addGroupDto.StudentIds is { Count: 0 })
                {
                    logger.LogInformation("Assigning {StudentCount} students to group {GroupId}",
                        addGroupDto.StudentIds.Count, group.GroupId);

                    // Get all valid students that belong to the school
                    var validStudents = await context.Students
                        .Where(s => addGroupDto.StudentIds.Contains(s.StudentId) &&
                                  s.SchoolId == parsedSchoolId &&
                                  !s.GroupId.HasValue)
                        .ToListAsync();

                    // Log if any student IDs were invalid
                    var invalidStudentIds = addGroupDto.StudentIds
                        .Except(validStudents.Select(s => s.StudentId))
                        .ToList();

                    if (invalidStudentIds.Any())
                    {
                        logger.LogWarning("Some student IDs are invalid or already in another group: {InvalidIds}",
                            string.Join(", ", invalidStudentIds));
                    }

                    // Assign the valid students to this group
                    foreach (var student in validStudents)
                    {
                        student.GroupId = group.GroupId;
                    }
                }

                await context.SaveChangesAsync();
                logger.LogInformation("Successfully created group {GroupId} with name {GroupName}",
                    group.GroupId, group.GroupName);

                // Prepare response DTO
                var students = await context.Students
                    .Where(s => s.GroupId == group.GroupId)
                    .Include(s => s.Parent)
                        .ThenInclude(p => p.User)
                    .Select(s => new GetGroupStudetDto
                    {
                        StudentId = s.StudentId,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                   
                    })
                    .ToListAsync();
                var specializationName  = await context.Specializations
                    .Where(s => s.SpecializationId == classroom.SpecializationId)
                    .Select(s => s.Name)
                    .FirstOrDefaultAsync();
                var schoolType = classroom.SchoolLevel.SchoolType.Name;
                var responseDto = new GroupDto
                {
                    GroupId = group.GroupId,
                    GroupName = group.GroupName,
                    LevelId = classroom.SchoolLevelId,
                    GroupCapacity = group.GroupCapacity,
                    SchoolId = group.SchoolId,
                    Level = new LevelDto
                    {
                        LevelId = classroom.SchoolLevelId,
                        Year = classroom.SchoolLevel.LevelYear,
                        Specialization = specializationName,
                        SchoolType = schoolType
                    },
                    Students = students
                };

                return result.Success(responseDto, (int)HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while creating group: {ErrorMessage}", ex.Message);
                return result.Failure("An error occurred while creating the group", (int)HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<List<GroupListingDto>, string>> GetGroupsByLevelIdAsync(int levelId, string schoolId)
        {
            var result = new Result<List<GroupListingDto>, string>();

            try
            {
                logger.LogInformation("Getting groups for school {SchoolId} and level {LevelId}", schoolId, levelId);

                if (!Guid.TryParse(schoolId, out var parsedSchoolId))
                {
                    logger.LogWarning("Invalid school ID provided: {SchoolId}", schoolId);
                    return result.Failure("Invalid school ID", (int)HttpStatusCode.BadRequest);
                }

                // Verify if level exists
                bool levelExists = await context.SchoolLevels.AnyAsync(l => l.LevelId == levelId);
                if (!levelExists)
                {
                    logger.LogWarning("School level with ID {LevelId} not found", levelId);
                    return result.Failure("School level not found", (int)HttpStatusCode.NotFound);
                }

                // Get classrooms associated with this level
                var classroomIds = await context.Classrooms
                    .Where(c => c.SchoolId == parsedSchoolId && c.SchoolLevelId == levelId)
                    .Select(c => c.ClassroomId)
                    .ToListAsync();

            var groups = await context.Groups
                        .Where(g => g.SchoolId == parsedSchoolId && classroomIds.Contains(g.ClassroomId))
                        .Include(g => g.Classroom)
                            .ThenInclude(c => c!.SchoolLevel)
                        .Select(g => new
                        {
                            g.GroupId,
                            g.GroupName,
                            g.Classroom!.SchoolLevelId,
                            g.GroupCapacity,
                            StudentCount = context.Students.Count(s => s.GroupId == g.GroupId),
                            g.Classroom.SchoolLevel.LevelYear
                        })
                        .ToListAsync();
                    
                    var groupDtos = groups.Select(g => new GroupListingDto
                    {
                        GroupId = g.GroupId,
                        GroupName = g.GroupName,
                        LevelId = g.SchoolLevelId,
                        GroupCapacity = g.GroupCapacity,
                        StudentCount = g.StudentCount,
                        Level = new LevelDto
                        {
                            LevelId = g.SchoolLevelId,
                            
                        }
                    }).ToList();

                logger.LogInformation("Retrieved {GroupCount} groups for school {SchoolId} and level {LevelId}",
                    groups.Count, parsedSchoolId, levelId);

                return result.Success(groupDtos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while retrieving groups by level: {ErrorMessage}", ex.Message);
                return result.Failure("An error occurred while retrieving the groups", (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}