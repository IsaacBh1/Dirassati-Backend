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
                var specializationName = await context.Specializations
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

        public async Task<Result<List<GroupListingDto>, string>> GetAllGroupsOrByLevelIdAsync(int? levelId, string schoolId)
        {
            var result = new Result<List<GroupListingDto>, string>();

            try
            {
                logger.LogInformation("Getting groups for school {SchoolId} {LevelFilter}",
                    schoolId, levelId.HasValue ? $"filtered by level {levelId}" : "for all levels");

                // Validate school ID format
                if (!Guid.TryParse(schoolId, out var parsedSchoolId))
                {
                    logger.LogWarning("Invalid school ID format: {SchoolId}", schoolId);
                    return result.Failure("Invalid school ID format", (int)HttpStatusCode.BadRequest);
                }

                // Verify school exists
                bool schoolExists = await context.Schools.AnyAsync(s => s.SchoolId == parsedSchoolId);
                if (!schoolExists)
                {
                    logger.LogWarning("School with ID {SchoolId} not found", schoolId);
                    return result.Failure("School not found", (int)HttpStatusCode.NotFound);
                }

                // Verify level exists if provided
                if (levelId.HasValue)
                {
                    bool levelExists = await context.SchoolLevels.AnyAsync(l => l.LevelId == levelId.Value);
                    if (!levelExists)
                    {
                        logger.LogWarning("School level with ID {LevelId} not found", levelId.Value);
                        return result.Failure("School level not found", (int)HttpStatusCode.NotFound);
                    }
                }

                // Get classroom IDs based on filter criteria



                List<Guid> classroomIds = [];
                if (levelId is not null)
                {

                    classroomIds = await context.Classrooms
                       .Where(c => c.SchoolId == parsedSchoolId && c.SchoolLevelId == levelId).Select(c => c.ClassroomId).ToListAsync();
                    // If no classrooms match the criteria
                    if (classroomIds.Count == 0)
                    {
                        logger.LogInformation("No classrooms found for the specified criteria. " +
                            "SchoolId: {SchoolId}, LevelId: {LevelId}", schoolId, levelId);
                        return result.Success([]);
                    }
                }



                // Get groups with their details
                var groups = await context.Groups
                    .Where(g => levelId != null ? g.SchoolId == parsedSchoolId && classroomIds.Contains(g.ClassroomId) : g.SchoolId == parsedSchoolId)
                    .Include(g => g.Classroom)
                        .ThenInclude(c => c!.SchoolLevel)

                    .Select(g => new
                    {
                        g.GroupId,
                        g.GroupName,
                        g.Classroom!.SchoolLevelId,
                        g.GroupCapacity,
                        StudentCount = context.Students.Count(s => s.GroupId == g.GroupId),
                        g.Classroom.SchoolLevel.LevelYear,
                        SchoolTypeName = g.Classroom.SchoolLevel.SchoolType.Name,
                        SpecializationName = g.Classroom.SpecializationId.HasValue ?
                            context.Specializations
                                .Where(s => s.SpecializationId == g.Classroom.SpecializationId)
                                .Select(s => s.Name)
                                .FirstOrDefault() :
                            null
                    })
                    .ToListAsync();

                // Map to DTOs with full level information
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
                        Year = g.LevelYear,
                        SchoolType = g.SchoolTypeName,
                        Specialization = g.SpecializationName
                    }
                }).ToList();

                logger.LogInformation("Retrieved {GroupCount} groups for school {SchoolId} {LevelInfo}",
                    groups.Count, parsedSchoolId, levelId.HasValue ? $"for level {levelId}" : "across all levels");

                return result.Success(groupDtos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while retrieving groups: {ErrorMessage}", ex.Message);
                return result.Failure("An error occurred while retrieving the groups", (int)HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<GroupDto, string>> UpdateGroupAsync(Guid groupId, UpdateGroupDto updateGroupDto, string schoolId)
        {
            var result = new Result<GroupDto, string>();

            try
            {
                logger.LogInformation("Updating group {GroupId}", groupId);

                if (!Guid.TryParse(schoolId, out var parsedSchoolId))
                {
                    logger.LogWarning("Invalid school ID format: {SchoolId}", schoolId);
                    return result.Failure("Invalid school ID format", (int)HttpStatusCode.BadRequest);
                }

                // Find the group with its related data
                var group = await context.Groups
                    .Include(g => g.Classroom)
                        .ThenInclude(c => c!.SchoolLevel)
                            .ThenInclude(sl => sl.SchoolType)
                    .Include(g => g.Students)
                    .FirstOrDefaultAsync(g => g.GroupId == groupId && g.SchoolId == parsedSchoolId);

                if (group == null)
                {
                    logger.LogWarning("Group {GroupId} not found in school {SchoolId}", groupId, schoolId);
                    return result.Failure("Group not found", (int)HttpStatusCode.NotFound);
                }

                // Update group name if provided
                if (!string.IsNullOrWhiteSpace(updateGroupDto.GroupName) &&
                    updateGroupDto.GroupName != group.GroupName)
                {
                    // Check if group name already exists in the school
                    var groupNameExists = await context.Groups
                        .AnyAsync(g => g.SchoolId == parsedSchoolId &&
                                    g.GroupId != groupId &&
                                    g.GroupName.ToLower() == updateGroupDto.GroupName.ToLower());

                    if (groupNameExists)
                    {
                        logger.LogWarning("Group with name {GroupName} already exists in school {SchoolId}",
                            updateGroupDto.GroupName, parsedSchoolId);
                        return result.Failure("A group with this name already exists in your school",
                            (int)HttpStatusCode.BadRequest);
                    }

                    group.GroupName = updateGroupDto.GroupName;
                    logger.LogInformation("Updated group name to {GroupName}", updateGroupDto.GroupName);
                }

                // Update capacity if provided
                if (updateGroupDto.GroupCapacity.HasValue && updateGroupDto.GroupCapacity > 0)
                {
                    var currentStudentCount = group.Students.Count;

                    // Ensure new capacity is not less than the current student count
                    if (updateGroupDto.GroupCapacity < currentStudentCount)
                    {
                        logger.LogWarning("Requested capacity {RequestedCapacity} is less than current student count {StudentCount}",
                            updateGroupDto.GroupCapacity, currentStudentCount);
                        return result.Failure($"Group capacity cannot be less than the current number of students ({currentStudentCount})",
                            (int)HttpStatusCode.BadRequest);
                    }

                    group.GroupCapacity = updateGroupDto.GroupCapacity.Value;
                    logger.LogInformation("Updated group capacity to {GroupCapacity}", updateGroupDto.GroupCapacity.Value);
                }

                // Add new students if provided
                if (updateGroupDto.StudentIds != null && updateGroupDto.StudentIds.Any())
                {
                    // Get current student count to check capacity
                    var currentStudentCount = await context.Students
                        .CountAsync(s => s.GroupId == groupId);

                    // Check if adding these students would exceed capacity
                    var newStudents = await context.Students
                        .Where(s => updateGroupDto.StudentIds.Contains(s.StudentId) &&
                                   s.SchoolId == parsedSchoolId &&
                                   (!s.GroupId.HasValue || s.GroupId != groupId))
                        .ToListAsync();

                    if (currentStudentCount + newStudents.Count > group.GroupCapacity)
                    {
                        logger.LogWarning("Cannot add students - would exceed capacity of {Capacity}", group.GroupCapacity);
                        return result.Failure(
                            $"Adding these students would exceed the group capacity of {group.GroupCapacity}. Current count: {currentStudentCount}, " +
                            $"attempting to add: {newStudents.Count}",
                            (int)HttpStatusCode.BadRequest);
                    }

                    // Assign the students to this group
                    foreach (var student in newStudents)
                    {
                        student.GroupId = groupId;
                        logger.LogInformation("Assigned student {StudentId} to group {GroupId}",
                            student.StudentId, groupId);
                    }

                    // Log if any student IDs were invalid
                    var invalidStudentIds = updateGroupDto.StudentIds
                        .Except(newStudents.Select(s => s.StudentId))
                        .ToList();

                    if (invalidStudentIds.Any())
                    {
                        logger.LogWarning("Some student IDs are invalid or already in the group: {InvalidIds}",
                            string.Join(", ", invalidStudentIds));
                    }
                }

                // Save changes
                await context.SaveChangesAsync();

                // Prepare response DTO with updated information
                var students = await context.Students
                    .Where(s => s.GroupId == groupId)
                    .Select(s => new GetGroupStudetDto
                    {
                        StudentId = s.StudentId,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                    })
                    .ToListAsync();

                var specializationName = group.Classroom?.SpecializationId.HasValue == true ?
                    await context.Specializations
                        .Where(s => s.SpecializationId == group.Classroom.SpecializationId)
                        .Select(s => s.Name)
                        .FirstOrDefaultAsync() : null;

                var responseDto = new GroupDto
                {
                    GroupId = group.GroupId,
                    GroupName = group.GroupName,
                    LevelId = group.Classroom!.SchoolLevelId,
                    GroupCapacity = group.GroupCapacity,
                    SchoolId = group.SchoolId,
                    Level = new LevelDto
                    {
                        LevelId = group.Classroom.SchoolLevelId,
                        Year = group.Classroom.SchoolLevel.LevelYear,
                        Specialization = specializationName,
                        SchoolType = group.Classroom.SchoolLevel.SchoolType.Name
                    },
                    Students = students
                };

                logger.LogInformation("Successfully updated group {GroupId}", groupId);
                return result.Success(responseDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while updating group: {ErrorMessage}", ex.Message);
                return result.Failure("An error occurred while updating the group",
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<Unit, string>> DeleteGroupAsync(Guid groupId, string schoolId)
        {
            var result = new Result<Unit, string>();

            try
            {
                logger.LogInformation("Deleting group {GroupId}", groupId);

                if (!Guid.TryParse(schoolId, out var parsedSchoolId))
                {
                    logger.LogWarning("Invalid school ID format: {SchoolId}", schoolId);
                    return result.Failure("Invalid school ID format", (int)HttpStatusCode.BadRequest);
                }

                // Find the group
                var group = await context.Groups
                    .Include(g => g.Students)
                    .FirstOrDefaultAsync(g => g.GroupId == groupId && g.SchoolId == parsedSchoolId);

                if (group == null)
                {
                    logger.LogWarning("Group {GroupId} not found in school {SchoolId}", groupId, schoolId);
                    return result.Failure("Group not found", (int)HttpStatusCode.NotFound);
                }

                // Check if group has students
                if (group.Students.Count > 0)
                {
                    // Remove group association from students
                    foreach (var student in group.Students)
                    {
                        student.GroupId = null;
                    }
                    logger.LogInformation("Removed {StudentCount} students from group {GroupId}",
                        group.Students.Count, groupId);
                }

                // Delete the group
                context.Groups.Remove(group);
                await context.SaveChangesAsync();

                logger.LogInformation("Successfully deleted group {GroupId}", groupId);
                return result.Success(Unit.Value);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while deleting group: {ErrorMessage}", ex.Message);
                return result.Failure("An error occurred while deleting the group",
                    (int)HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<GroupDto, string>> AssignStudentToGroupAsync(AssignStudentToGroupDto assignDto, string schoolId)
        {
            var result = new Result<GroupDto, string>();

            try
            {
                logger.LogInformation("Assigning student {StudentId} to group {GroupId}",
                    assignDto.StudentId, assignDto.GroupId);

                if (!Guid.TryParse(schoolId, out var parsedSchoolId))
                {
                    logger.LogWarning("Invalid school ID format: {SchoolId}", schoolId);
                    return result.Failure("Invalid school ID format", (int)HttpStatusCode.BadRequest);
                }

                // Find the group with related data
                var group = await context.Groups
                    .Include(g => g.Classroom)
                        .ThenInclude(c => c!.SchoolLevel)
                            .ThenInclude(sl => sl.SchoolType)
                    .Include(g => g.Students)
                    .FirstOrDefaultAsync(g => g.GroupId == assignDto.GroupId && g.SchoolId == parsedSchoolId);

                if (group == null)
                {
                    logger.LogWarning("Group {GroupId} not found in school {SchoolId}",
                        assignDto.GroupId, schoolId);
                    return result.Failure("Group not found", (int)HttpStatusCode.NotFound);
                }

                // Find the student
                var student = await context.Students
                    .FirstOrDefaultAsync(s => s.StudentId == assignDto.StudentId && s.SchoolId == parsedSchoolId);

                if (student == null)
                {
                    logger.LogWarning("Student {StudentId} not found in school {SchoolId}",
                        assignDto.StudentId, schoolId);
                    return result.Failure("Student not found", (int)HttpStatusCode.NotFound);
                }

                // Check if student is already in this group
                if (student.GroupId == assignDto.GroupId)
                {
                    logger.LogWarning("Student {StudentId} is already assigned to group {GroupId}",
                        assignDto.StudentId, assignDto.GroupId);
                    return result.Failure("Student is already assigned to this group",
                        (int)HttpStatusCode.BadRequest);
                }

                // Check if the group has room for the student
                var currentStudentCount = await context.Students
                    .CountAsync(s => s.GroupId == assignDto.GroupId);

                if (currentStudentCount >= group.GroupCapacity)
                {
                    logger.LogWarning("Group {GroupId} is at maximum capacity of {Capacity}",
                        assignDto.GroupId, group.GroupCapacity);
                    return result.Failure($"Group is at maximum capacity ({group.GroupCapacity})",
                        (int)HttpStatusCode.BadRequest);
                }

                // Ensure the student's level matches the group's level
                if (student.SchoolLevelId != group.Classroom!.SchoolLevelId)
                {
                    logger.LogWarning("Student level {StudentLevel} does not match group level {GroupLevel}",
                        student.SchoolLevelId, group.Classroom.SchoolLevelId);
                    return result.Failure("Student's level does not match the group's level",
                        (int)HttpStatusCode.BadRequest);
                }

                // Assign the student to the group
                student.GroupId = assignDto.GroupId;
                await context.SaveChangesAsync();
                logger.LogInformation("Successfully assigned student {StudentId} to group {GroupId}",
                    assignDto.StudentId, assignDto.GroupId);

                // Prepare response DTO with updated information
                var students = await context.Students
                    .Where(s => s.GroupId == assignDto.GroupId)
                    .Select(s => new GetGroupStudetDto
                    {
                        StudentId = s.StudentId,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                    })
                    .ToListAsync();

                var specializationName = group.Classroom?.SpecializationId.HasValue == true ?
                    await context.Specializations
                        .Where(s => s.SpecializationId == group.Classroom.SpecializationId)
                        .Select(s => s.Name)
                        .FirstOrDefaultAsync() : null;

                var responseDto = new GroupDto
                {
                    GroupId = group.GroupId,
                    GroupName = group.GroupName,
                    LevelId = group.Classroom!.SchoolLevelId,
                    GroupCapacity = group.GroupCapacity,
                    SchoolId = group.SchoolId,
                    Level = new LevelDto
                    {
                        LevelId = group.Classroom.SchoolLevelId,
                        Year = group.Classroom.SchoolLevel.LevelYear,
                        Specialization = specializationName,
                        SchoolType = group.Classroom.SchoolLevel.SchoolType.Name
                    },
                    Students = students
                };

                return result.Success(responseDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while assigning student to group: {ErrorMessage}", ex.Message);
                return result.Failure("An error occurred while assigning the student to the group",
                    (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}