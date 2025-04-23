using Dirassati_Backend.Common;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Classrooms.Dtos;
using Dirassati_Backend.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Dirassati_Backend.Data.Enums;

namespace Dirassati_Backend.Features.Classrooms.Services
{
    public class ClassroomServices(AppDbContext context, ILogger<ClassroomServices> logger) : IClassroomServices
    {
        public async Task<Result<ClassroomDto, string>> AddClassroomAsync(AddClassroomDto addClassroomDto, string schoolId)
        {
            var result = new Result<ClassroomDto, string>();

            try
            {
                logger.LogInformation("Adding new classroom with name: {ClassName}", addClassroomDto.ClassName);

                if (!Guid.TryParse(schoolId, out var parsedSchoolId))
                {
                    logger.LogWarning("Invalid school ID provided: {SchoolId}", schoolId);
                    return result.Failure("Invalid school ID", (int)HttpStatusCode.BadRequest);

                }

                // Check if school exists
                var school = await context.Schools.FindAsync(parsedSchoolId);
                if (school == null)
                {
                    logger.LogWarning("School with ID {SchoolId} not found", parsedSchoolId);
                    return result.Failure("School not found", (int)HttpStatusCode.NotFound);
                }

                // Check if school level exists
                var level = await context.SchoolLevels
                    .Include(l => l.SchoolType)
                    .FirstOrDefaultAsync(l => l.LevelId == addClassroomDto.SchoolLevelId);

                if (level == null)
                {
                    logger.LogWarning("School level with ID {LevelId} not found", addClassroomDto.SchoolLevelId);
                    return result.Failure("School level not found", (int)HttpStatusCode.NotFound);
                }
                if (level.SchoolTypeId != school.SchoolTypeId)
                {
                    logger.LogWarning("School level {LevelId} does not match school type {Type}",
                        addClassroomDto.SchoolLevelId, school.SchoolTypeId);
                    return result.Failure("School level does not match school type", (int)HttpStatusCode.BadRequest);
                }
                // Check if specialization exists

                var specialization = await context.Specializations.FindAsync(addClassroomDto.SpecializationId);
                if (school.SchoolTypeId == (int)SchoolTypeEnum.Lycee && specialization == null)
                {
                    logger.LogWarning("Specialization required for school type {Type} ", SchoolTypeEnum.Lycee);
                    return result.Failure($"Specialization required for school type {SchoolTypeEnum.Lycee}", (int)HttpStatusCode.NotFound);
                }
                else if (school.SchoolTypeId != (int)SchoolTypeEnum.Lycee && specialization != null)
                {
                    logger.LogWarning("Specialization not allowed for school type {Type} ", SchoolTypeEnum.Lycee);
                    return result.Failure($"Specialization not allowed for school type {SchoolTypeEnum.Lycee}", (int)HttpStatusCode.NotFound);
                }

                // Check if the classroom name is already taken in this school
                var existingClassroom = await context.Classrooms
                    .FirstOrDefaultAsync(c => c.SchoolId == parsedSchoolId &&
                                             c.ClassName.ToLower() == addClassroomDto.ClassName.ToLower());
                if (existingClassroom != null)
                {
                    logger.LogWarning("Classroom with name {ClassName} already exists in school {SchoolId}",
                        addClassroomDto.ClassName, parsedSchoolId);
                    return result.Failure("A classroom with this name already exists",
                        (int)HttpStatusCode.BadRequest);
                }

                var classroom = new Classroom
                {
                    ClassroomId = Guid.NewGuid(),
                    ClassName = addClassroomDto.ClassName,
                    SchoolId = parsedSchoolId,
                    SchoolLevelId = addClassroomDto.SchoolLevelId,
                    SpecializationId = addClassroomDto.SpecializationId
                };

                context.Classrooms.Add(classroom);
                await context.SaveChangesAsync();

                logger.LogInformation("Successfully created classroom {ClassroomId} with name {ClassName}",
                    classroom.ClassroomId, classroom.ClassName);

                var classroomDto = new ClassroomDto
                {
                    ClassroomId = classroom.ClassroomId,
                    ClassName = classroom.ClassName,
                    SchoolLevelId = classroom.SchoolLevelId,
                    SpecializationId = classroom.SpecializationId,
                    LevelName = $"Year {level.LevelYear}",
                    SpecializationName = specialization?.Name ?? string.Empty,
                    SchoolType = level.SchoolType.Name
                };

                return result.Success(classroomDto, (int)HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while creating classroom: {ErrorMessage}", ex.Message);
                return result.Failure("An error occurred while creating the classroom", (int)HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<List<ClassroomDto>, string>> GetClassroomsBySchoolLevelAsync(int levelId, string schoolId)
        {
            var result = new Result<List<ClassroomDto>, string>();

            try
            {
                logger.LogInformation("Getting classrooms for school {SchoolId} and level {LevelId}", schoolId, levelId);

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

                var classrooms = await context.Classrooms
                    .Where(c => c.SchoolId == parsedSchoolId && c.SchoolLevelId == levelId)
                    .Include(c => c.SchoolLevel)
                        .ThenInclude(l => l.SchoolType)
                    .Include(c => c.Specialization)
                    .Select(c => new ClassroomDto
                    {
                        ClassroomId = c.ClassroomId,
                        ClassName = c.ClassName,
                        SchoolLevelId = c.SchoolLevelId,
                        SpecializationId = c.SpecializationId,
                        LevelName = $"Year {c.SchoolLevel.LevelYear}",
                        SpecializationName = c.Specialization != null ? c.Specialization.Name : string.Empty,
                        SchoolType = c.SchoolLevel.SchoolType.Name
                    })
                    .ToListAsync();

                logger.LogInformation("Retrieved {ClassroomCount} classrooms for school {SchoolId} and level {LevelId}",
                    classrooms.Count, parsedSchoolId, levelId);

                return result.Success(classrooms);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while retrieving classrooms by level: {ErrorMessage}", ex.Message);
                return result.Failure("An error occurred while retrieving the classrooms", (int)HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<List<ClassroomDto>, string>> GetAllClassroomsBySchoolAsync(string schoolId)
        {
            var result = new Result<List<ClassroomDto>, string>();

            try
            {
                logger.LogInformation("Getting all classrooms for school {SchoolId}", schoolId);

                if (!Guid.TryParse(schoolId, out var parsedSchoolId))
                {
                    logger.LogWarning("Invalid school ID provided: {SchoolId}", schoolId);
                    return result.Failure("Invalid school ID", (int)HttpStatusCode.BadRequest);
                }

                var classrooms = await context.Classrooms
                    .Where(c => c.SchoolId == parsedSchoolId)
                    .Include(c => c.SchoolLevel)
                        .ThenInclude(l => l.SchoolType)
                    .Include(c => c.Specialization)
                    .Select(c => new ClassroomDto
                    {
                        ClassroomId = c.ClassroomId,
                        ClassName = c.ClassName,
                        SchoolLevelId = c.SchoolLevelId,
                        SpecializationId = c.SpecializationId,
                        LevelName = $"Year {c.SchoolLevel.LevelYear}",
                        SpecializationName = c.Specialization != null ? c.Specialization.Name : string.Empty,
                        SchoolType = c.SchoolLevel.SchoolType.Name
                    })
                    .ToListAsync();

                logger.LogInformation("Retrieved {ClassroomCount} classrooms for school {SchoolId}",
                    classrooms.Count, parsedSchoolId);

                return result.Success(classrooms);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while retrieving all classrooms: {ErrorMessage}", ex.Message);
                return result.Failure("An error occurred while retrieving the classrooms", (int)HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<ClassroomDetailDto, string>> GetClassroomDetailsAsync(Guid classroomId, string schoolId)
        {
            var result = new Result<ClassroomDetailDto, string>();

            try
            {
                logger.LogInformation("Getting detailed information for classroom {ClassroomId}", classroomId);

                if (!Guid.TryParse(schoolId, out var parsedSchoolId))
                {
                    logger.LogWarning("Invalid school ID provided: {SchoolId}", schoolId);
                    return result.Failure("Invalid school ID", (int)HttpStatusCode.BadRequest);
                }

                var classroom = await context.Classrooms
                    .Include(c => c.SchoolLevel)
                        .ThenInclude(l => l.SchoolType)
                    .Include(c => c.Specialization)
                    .Include(c => c.Group)
                    .FirstOrDefaultAsync(c => c.ClassroomId == classroomId && c.SchoolId == parsedSchoolId);

                if (classroom == null)
                {
                    logger.LogWarning("Classroom with ID {ClassroomId} not found in school {SchoolId}",
                        classroomId, parsedSchoolId);
                    return result.Failure("Classroom not found", (int)HttpStatusCode.NotFound);
                }

                ClassroomGroupDto? groupDto = null;

                if (classroom.Group != null)
                {
                    int studentCount = await context.Students.CountAsync(student => student.GroupId == classroom.Group.GroupId);

                    groupDto = new ClassroomGroupDto
                    {
                        GroupId = classroom.Group.GroupId,
                        GroupName = classroom.Group.GroupName,
                        StudentCount = studentCount,
                        GroupCapacity = classroom.Group.GroupCapacity
                    };
                }

                var classroomDetailDto = new ClassroomDetailDto
                {
                    ClassroomId = classroom.ClassroomId,
                    ClassName = classroom.ClassName,
                    SchoolLevelId = classroom.SchoolLevelId,
                    SpecializationId = classroom.SpecializationId,
                    LevelName = $"Year {classroom.SchoolLevel.LevelYear}",
                    SpecializationName = classroom.Specialization?.Name ?? string.Empty,
                    SchoolType = classroom.SchoolLevel.SchoolType.Name,
                    Group = groupDto
                };

                return result.Success(classroomDetailDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while retrieving classroom details: {ErrorMessage}", ex.Message);
                return result.Failure("An error occurred while retrieving the classroom details", (int)HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<ClassroomDto, string>> UpdateClassroomAsync(Guid classroomId, UpdateClassroomDto updateClassroomDto, string schoolId)
        {
            var result = new Result<ClassroomDto, string>();

            try
            {
                logger.LogInformation("Updating classroom {ClassroomId}", classroomId);

                if (!Guid.TryParse(schoolId, out var parsedSchoolId))
                {
                    logger.LogWarning("Invalid school ID provided: {SchoolId}", schoolId);
                    return result.Failure("Invalid school ID", (int)HttpStatusCode.BadRequest);
                }

                var classroom = await context.Classrooms
                    .Include(c => c.SchoolLevel)
                        .ThenInclude(l => l.SchoolType)
                    .Include(c => c.Specialization)
                    .Include(c => c.Group)
                    .FirstOrDefaultAsync(c => c.ClassroomId == classroomId && c.SchoolId == parsedSchoolId);

                if (classroom == null)
                {
                    logger.LogWarning("Classroom with ID {ClassroomId} not found in school {SchoolId}",
                        classroomId, parsedSchoolId);
                    return result.Failure("Classroom not found", (int)HttpStatusCode.NotFound);
                }

                // Check if name is already taken (if changing name)
                if (classroom.ClassName != updateClassroomDto.ClassName)
                {
                    var nameExists = await context.Classrooms
                        .AnyAsync(c => c.SchoolId == parsedSchoolId &&
                                     c.ClassName.ToLower() == updateClassroomDto.ClassName.ToLower() &&
                                     c.ClassroomId != classroomId);

                    if (nameExists)
                    {
                        logger.LogWarning("Classroom with name {ClassName} already exists in school {SchoolId}",
                            updateClassroomDto.ClassName, parsedSchoolId);
                        return result.Failure("A classroom with this name already exists", (int)HttpStatusCode.BadRequest);
                    }
                }

                // Update name
                classroom.ClassName = updateClassroomDto.ClassName;

                // Handle school level update if requested
                if (updateClassroomDto.SchoolLevelId.HasValue &&
                    updateClassroomDto.SchoolLevelId.Value != classroom.SchoolLevelId)
                {
                    // Check if any groups are associated with this classroom
                    if (classroom.Group != null)
                    {
                        logger.LogWarning("Cannot update school level for classroom {ClassroomId} because it has associated groups",
                            classroomId);
                        return result.Failure("Cannot update school level because this classroom has associated groups. " +
                            "Please delete or reassign all groups first.", (int)HttpStatusCode.BadRequest);
                    }

                    // Verify if the new level exists
                    var newLevel = await context.SchoolLevels
                        .Include(l => l.SchoolType)
                        .FirstOrDefaultAsync(l => l.LevelId == updateClassroomDto.SchoolLevelId.Value);

                    if (newLevel == null)
                    {
                        logger.LogWarning("School level with ID {LevelId} not found", updateClassroomDto.SchoolLevelId.Value);
                        return result.Failure("The specified school level does not exist", (int)HttpStatusCode.BadRequest);
                    }

                    classroom.SchoolLevelId = updateClassroomDto.SchoolLevelId.Value;
                    classroom.SchoolLevel = newLevel; // Update the navigation property
                }

                // Handle specialization update if requested
                if (updateClassroomDto.SpecializationId.HasValue &&
                    updateClassroomDto.SpecializationId.Value != classroom.SpecializationId)
                {
                    var newSpecialization = await context.Specializations
                        .FindAsync(updateClassroomDto.SpecializationId.Value);

                    if (newSpecialization == null)
                    {
                        logger.LogWarning("Specialization with ID {SpecializationId} not found",
                            updateClassroomDto.SpecializationId.Value);
                        return result.Failure("The specified specialization does not exist", (int)HttpStatusCode.BadRequest);
                    }

                    classroom.SpecializationId = updateClassroomDto.SpecializationId.Value;
                    classroom.Specialization = newSpecialization; // Update the navigation property
                }

                await context.SaveChangesAsync();
                logger.LogInformation("Successfully updated classroom {ClassroomId}", classroomId);

                // Refresh data after update
                var classroomDto = new ClassroomDto
                {
                    ClassroomId = classroom.ClassroomId,
                    ClassName = classroom.ClassName,
                    SchoolLevelId = classroom.SchoolLevelId,
                    SpecializationId = classroom.SpecializationId,
                    LevelName = $"Year {classroom.SchoolLevel.LevelYear}",
                    SpecializationName = classroom.Specialization?.Name ?? string.Empty,
                    SchoolType = classroom.SchoolLevel.SchoolType.Name
                };

                return result.Success(classroomDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while updating classroom: {ErrorMessage}", ex.Message);
                return result.Failure("An error occurred while updating the classroom", (int)HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<Unit, string>> DeleteClassroomAsync(Guid classroomId, string schoolId)
        {
            var result = new Result<Unit, string>();

            try
            {
                logger.LogInformation("Attempting to delete classroom with ID: {ClassroomId}", classroomId);

                if (!Guid.TryParse(schoolId, out var parsedSchoolId))
                {
                    logger.LogWarning("Invalid school ID provided: {SchoolId}", schoolId);
                    return result.Failure("Invalid school ID", (int)HttpStatusCode.BadRequest);
                }

                // Check if classroom exists and belongs to the specified school
                var classroom = await context.Classrooms
                    .Include(c => c.Group)
                    .FirstOrDefaultAsync(c => c.ClassroomId == classroomId && c.SchoolId == parsedSchoolId);

                if (classroom == null)
                {
                    logger.LogWarning("Classroom with ID {ClassroomId} not found in school {SchoolId}",
                        classroomId, parsedSchoolId);
                    return result.Failure("Classroom not found", (int)HttpStatusCode.NotFound);
                }

                // Check if classroom has an associated group
                if (classroom.Group != null)
                {
                    logger.LogWarning("Cannot delete classroom {ClassroomId} because it has an associated group {GroupId}",
                        classroomId, classroom.Group.GroupId);
                    return result.Failure("Cannot delete classroom because it has an associated group. " +
                        "Please delete the group first.", (int)HttpStatusCode.BadRequest);
                }

                // Check if any lessons are associated with this classroom
                bool hasLessons = await context.Lessons.AnyAsync(l => l.ClassroomId == classroomId);
                if (hasLessons)
                {
                    logger.LogWarning("Cannot delete classroom {ClassroomId} because it has scheduled lessons",
                        classroomId);
                    return result.Failure("Cannot delete classroom because it has scheduled lessons. " +
                        "Please remove all associated lessons first.", (int)HttpStatusCode.BadRequest);
                }

                // Delete the classroom
                context.Classrooms.Remove(classroom);
                await context.SaveChangesAsync();

                logger.LogInformation("Successfully deleted classroom {ClassroomId}", classroomId);
                return result.Success(Unit.Value);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while deleting classroom: {ErrorMessage}", ex.Message);
                return result.Failure("An error occurred while deleting the classroom",
                    (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}