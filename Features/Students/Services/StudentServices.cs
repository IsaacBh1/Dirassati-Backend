using Dirassati_Backend.Common;
using Dirassati_Backend.Data.Enums;
using Dirassati_Backend.Features.Students.DTOs;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Dirassati_Backend.Features.Students.Services;

public class StudentServices(AppDbContext dbContext, ParentServices parentServices)
{
    private static bool IsFirstYearLycee(int schoolLevelId) =>
    schoolLevelId == (int)SchoolLevelsEnum.Lycee1er;

    private static bool IsSecondOrThirdYearLycee(int schoolLevelId) =>
        schoolLevelId == (int)SchoolLevelsEnum.Lycee2eme ||
        schoolLevelId == (int)SchoolLevelsEnum.Lycee3eme;

    private static bool IsBasicSpecialization(int? specializationId) =>
        specializationId == (int)SpecializationEnum.Lettres ||
        specializationId == (int)SpecializationEnum.Science;
    public async Task<Result<Guid, string>> AddStudentAsync(string schoolId, AddStudentDTO studentDTO)
    {
        var result = new Result<Guid, string>();

        // Validate school exists
        var school = await dbContext.Schools
            .FirstOrDefaultAsync(s => s.SchoolId.ToString() == schoolId);

        if (school == null)
            return result.Failure("School not found", 404);

        // Validate school level exists and belongs to this school type
        var schoolLevel = await dbContext.SchoolLevels
            .FirstOrDefaultAsync(l => l.LevelId == studentDTO.studentInfosDTO.SchoolLevelId);

        if (schoolLevel == null)
            return result.Failure("Invalid school level", 400);

        if (schoolLevel.SchoolTypeId != school.SchoolTypeId)
            return result.Failure("School level does not match school type", 400);

        // Validate specialization if provided
        if (studentDTO.studentInfosDTO.SpecializationId.HasValue)
        {
            var specialization = await dbContext.Specializations
                .FirstOrDefaultAsync(s => s.SpecializationId == studentDTO.studentInfosDTO.SpecializationId.Value);

            if (specialization == null)
                return result.Failure("Invalid specialization", 400);

            // Check if this school offers this specialization
            var hasSpecialization = await dbContext.Schools
                .Where(s => s.SchoolId.ToString() == schoolId)
                .SelectMany(s => s.Specializations)
                .AnyAsync(s => s.SpecializationId == studentDTO.studentInfosDTO.SpecializationId.Value);

            if (!hasSpecialization)
                return result.Failure("This specialization is not offered by this school", 400);

            var SpecializationId = studentDTO.studentInfosDTO.SpecializationId;
            var SchoolLevelId = studentDTO.studentInfosDTO.SchoolLevelId;
            if ((IsFirstYearLycee(SchoolLevelId) && !IsBasicSpecialization(SpecializationId)) ||
            (IsSecondOrThirdYearLycee(SchoolLevelId) && IsBasicSpecialization(SpecializationId)))
            {
                return result.Failure("Student Cannot Take This Specialization", 400);
            }

        }
        var relationshipExists = await dbContext.ParentRelationshipToStudentTypes.AnyAsync(r => r.Id == studentDTO.parentInfosDTO.RelationshipToStudentId);
        if (!relationshipExists)
            return result.Failure("Invalid parent relationship type", 400);

        using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            // Register or get parent and send confirmation email in case of a new parent
            var parentId = await parentServices.RegisterParent(
                studentDTO.parentInfosDTO.NationalIdentityNumber,
                studentDTO.parentInfosDTO
            );

            var student = new Data.Models.Student
            {
                FirstName = studentDTO.studentInfosDTO.FirstName,
                LastName = studentDTO.studentInfosDTO.LastName,
                Address = studentDTO.studentInfosDTO.Address,
                BirthDate = studentDTO.studentInfosDTO.BirthDate,
                BirthPlace = studentDTO.studentInfosDTO.BirthPlace,
                EmergencyContact = studentDTO.studentInfosDTO.EmergencyContact,
                SchoolLevelId = studentDTO.studentInfosDTO.SchoolLevelId,
                SpecializationId = studentDTO.studentInfosDTO.SpecializationId,

                EnrollmentDate = DateOnly.FromDateTime(DateTime.Now),
                ParentRelationshipToStudentTypeId = studentDTO.parentInfosDTO.RelationshipToStudentId,
                SchoolId = Guid.Parse(schoolId),
                ParentId = parentId,
                IsActive = true,
            };

            await dbContext.Students.AddAsync(student);

            await dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return result.Success(student.StudentId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return result.Failure($"Failed to add student: {ex.Message}", 500);
        }
    }

}