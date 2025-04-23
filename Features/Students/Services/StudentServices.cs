using System.Net;
using Dirassati_Backend.Common;
using Dirassati_Backend.Data.Enums;
using Dirassati_Backend.Features.Students.DTOs;
using Dirassati_Backend.Features.Students.Models;
using Dirassati_Backend.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Features.Students.Services;

public class StudentServices(AppDbContext dbContext, ParentServices parentServices)
{
    private static bool IsFirstYearLycee(int schoolLevelId) =>
    schoolLevelId == (int)SchoolLevelsEnum.Lycee1er;

    private static bool IsSecondOrThirdYearLycee(int schoolLevelId) =>
        schoolLevelId == (int)SchoolLevelsEnum.Lycee2eme ||
        schoolLevelId == (int)SchoolLevelsEnum.Lycee3eme;

    private static bool IsBasicSpecialization(int? specializationId) =>
        specializationId is (int)SpecializationEnum.Lettres or (int)SpecializationEnum.Science;
    public async Task<Result<Guid, string>> AddStudentAsync(string schoolId, AddStudentDto studentDto)
    {
        var result = new Result<Guid, string>();
        if (!Guid.TryParse(schoolId, out var schoolIdGuid))
            return result.Failure("Invalid School Id", (int)HttpStatusCode.BadRequest);
        var school = await dbContext.Schools
            .FirstOrDefaultAsync(s => s.SchoolId == schoolIdGuid);

        if (school == null)
            return result.Failure("School not found", 404);

        var schoolLevel = await dbContext.SchoolLevels
            .FirstOrDefaultAsync(l => l.LevelId == studentDto.studentInfosDTO.SchoolLevelId);

        if (schoolLevel == null)
            return result.Failure("Invalid school level", 400);

        if (schoolLevel.SchoolTypeId != school.SchoolTypeId)
            return result.Failure("School level does not match school type", 400);

        if (studentDto.studentInfosDTO.SpecializationId.HasValue)
        {
            var specialization = await dbContext.Specializations
                .FirstOrDefaultAsync(s => s.SpecializationId == studentDto.studentInfosDTO.SpecializationId.Value);

            if (specialization == null)
                return result.Failure("Invalid specialization", 400);

            var hasSpecialization = await dbContext.Schools
                .Where(s => s.SchoolId == schoolIdGuid)
                .SelectMany(s => s.Specializations)
                .AnyAsync(s => s.SpecializationId == studentDto.studentInfosDTO.SpecializationId.Value);

            if (!hasSpecialization)
                return result.Failure("This specialization is not offered by this school", 400);

            var SpecializationId = studentDto.studentInfosDTO.SpecializationId;
            var SchoolLevelId = studentDto.studentInfosDTO.SchoolLevelId;
            if ((IsFirstYearLycee(SchoolLevelId) && !IsBasicSpecialization(SpecializationId)) ||
            (IsSecondOrThirdYearLycee(SchoolLevelId) && IsBasicSpecialization(SpecializationId)))
            {
                return result.Failure("Student Cannot Take This Specialization", 400);
            }

        }
        var relationshipExists = await dbContext.ParentRelationshipToStudentTypes.AnyAsync(r => r.Id == studentDto.parentInfosDTO.RelationshipToStudentId);
        if (!relationshipExists)
            return result.Failure("Invalid parent relationship type", 400);

        using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            // Check if a parent with the same national identity number already exists
            var existingParentId = await dbContext.Parents
                .Where(p => p.NationalIdentityNumber == studentDto.parentInfosDTO.NationalIdentityNumber)
                .Select(p => p.ParentId)
                .FirstOrDefaultAsync();

            // Register or get parent and send confirmation email in case of a new parent
            var parentId = await parentServices.RegisterParent(
                studentDto.parentInfosDTO.NationalIdentityNumber,
                studentDto.parentInfosDTO
            );

            // Check if student already exists with the same name and parent
            var isStudentExist = await dbContext.Students
                .AnyAsync(s => s.FirstName == studentDto.studentInfosDTO.FirstName &&
                             s.LastName == studentDto.studentInfosDTO.LastName &&
                             s.ParentId == parentId);

            // Check if another student exists with the same parent by national identity number
            if (!isStudentExist && existingParentId != Guid.Empty)
            {
                isStudentExist = await dbContext.Students
                    .AnyAsync(s => s.FirstName == studentDto.studentInfosDTO.FirstName &&
                                 s.LastName == studentDto.studentInfosDTO.LastName &&
                                 s.ParentId == existingParentId);
            }

            if (isStudentExist)
                return result.Failure("Student already exists", 400);

            var student = new Data.Models.Student
            {
                FirstName = studentDto.studentInfosDTO.FirstName,
                LastName = studentDto.studentInfosDTO.LastName,
                Address = studentDto.studentInfosDTO.Address,
                BirthDate = studentDto.studentInfosDTO.BirthDate,
                BirthPlace = studentDto.studentInfosDTO.BirthPlace,
                EmergencyContact = studentDto.studentInfosDTO.EmergencyContact,
                SchoolLevelId = studentDto.studentInfosDTO.SchoolLevelId,
                SpecializationId = studentDto.studentInfosDTO.SpecializationId,

                EnrollmentDate = DateOnly.FromDateTime(DateTime.Now),
                ParentRelationshipToStudentTypeId = studentDto.parentInfosDTO.RelationshipToStudentId,
                SchoolId = Guid.Parse(schoolId),
                ParentId = parentId,
                IsActive = true,
            };

            await dbContext.Students.AddAsync(student);

            await dbContext.SaveChangesAsync();

            if (parentServices.VerificationToken != "")
                await parentServices.SendConfirmationEmailAsync();
            await transaction.CommitAsync();
            return result.Success(student.StudentId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine(ex);
            return result.Failure($"Failed to add student: {ex.Message}", 500);
        }
    }

    public async Task<Result<StudentImportResultDto, string>> ImportStudentsFromCsvAsync(
        string schoolId, IFormFile csvFile, bool hasHeaders)
    {
        var result = new Result<StudentImportResultDto, string>();

        if (!Guid.TryParse(schoolId, out var schoolIdGuid))
            return result.Failure("Invalid School Id", (int)HttpStatusCode.BadRequest);

        var school = await dbContext.Schools
            .FirstOrDefaultAsync(s => s.SchoolId == schoolIdGuid);

        if (school == null)
            return result.Failure("School not found", 404);

        try
        {
            // Class to map CSV columns to
            List<StudentCsvRecord> studentRecords;

            // Read CSV file
            using (var reader = new StreamReader(csvFile.OpenReadStream(), encoding: null, // Auto-detect encoding including BOMs
                       detectEncodingFromByteOrderMarks: true,
                       bufferSize: 1024,
                       leaveOpen: false))
            using (var csv = new CsvHelper.CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = hasHeaders,
                TrimOptions = CsvHelper.Configuration.TrimOptions.Trim,
                MissingFieldFound = null
            }))
            {
                // Register class map if needed
                csv.Context.RegisterClassMap<StudentCsvRecordMap>();

                // Read all records
                studentRecords = csv.GetRecords<StudentCsvRecord>().ToList();
            }

            if (studentRecords.Count == 0)
                return result.Failure("No valid records found in CSV file", 400);

            // Process records
            var importResult = new StudentImportResultDto
            {
                TotalRecords = studentRecords.Count,
                SuccessfulImports = 0,
                FailedImports = 0,
                Errors = []
            };

            foreach (var record in studentRecords)
            {
                try
                {
                    var allSchoolLevels = await dbContext.SchoolLevels
                        .Where(s => s.SchoolTypeId == school.SchoolTypeId)
                        .ToListAsync();
                    // Validate the record
                    var validationErrors = ValidateStudentCsvRecord(record, school.SchoolTypeId);
                    if (validationErrors.Count != 0)
                    {
                        importResult.Errors.Add($"Row for student {record.StudentFirstName} {record.StudentLastName}: {string.Join(", ", validationErrors)}");
                        importResult.FailedImports++;
                        continue;
                    }
                    var schoolLevel = (allSchoolLevels.FirstOrDefault(s => s.LevelYear == ParseInt(record.LevelYear)));
                    if (schoolLevel == null)
                    {
                        importResult.Errors.Add($"Row for student {record.StudentFirstName} {record.StudentLastName}: Invalid school level");
                        importResult.FailedImports++;
                        continue;
                    }
                    // Map to DTO
                    var studentDto = new AddStudentDto
                    {
                        studentInfosDTO = new StudentInfosDto
                        {
                            FirstName = record.StudentFirstName,
                            LastName = record.StudentLastName,
                            Address = record.StudentAddress,
                            BirthDate = ParseDateOnly(record.StudentBirthDate),
                            BirthPlace = record.StudentBirthPlace,
                            EmergencyContact = record.EmergencyContact,
                            SchoolLevelId = schoolLevel.LevelId,
                            SpecializationId = string.IsNullOrWhiteSpace(record.SpecializationId) ? null : ParseInt(record.SpecializationId)
                        },
                        parentInfosDTO = new ParentInfosDto
                        {
                            NationalIdentityNumber = record.ParentNationalIdNumber,
                            FirstName = record.ParentFirstName,
                            LastName = record.ParentLastName,
                            Email = record.ParentEmail,
                            RelationshipToStudentId = ParseInt(record.RelationshipToStudentId),
                            Occupation = record.ParentOccupation,
                            PhoneNumber = record.ParentPhoneNumber
                        }
                    };

                    // Add the student
                    var addResult = await AddStudentAsync(schoolId, studentDto);
                    if (addResult.IsSuccess)
                        importResult.SuccessfulImports++;
                    else
                    {
                        importResult.FailedImports++;
                        importResult.Errors.Add($"Row for student {record.StudentFirstName} {record.StudentLastName}: {addResult.Errors}");
                    }
                }
                catch (Exception ex)
                {
                    importResult.FailedImports++;
                    importResult.Errors.Add($"Row for student {record.StudentFirstName} {record.StudentLastName}: {ex.Message}");
                }
            }

            return result.Success(importResult);
        }
        catch (Exception ex)
        {
            return result.Failure($"Error processing CSV file: {ex.Message}", 500);
        }
    }

    public async Task<Result<Unit, string>> UpdateStudentAsync(Guid studentId, UpdateStudentDto studentDto)
    {
        var result = new Result<Unit, string>();

        try
        {
            // Check if student exists
            var student = await dbContext.Students
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
                return result.Failure("Student not found", 404);

            // Validate school level
            var schoolLevel = await dbContext.SchoolLevels
                .FirstOrDefaultAsync(l => l.LevelId == studentDto.SchoolLevelId);

            if (schoolLevel == null)
                return result.Failure("Invalid school level", 400);

            // Check if school level matches school type
            var schoolTypeId = await dbContext.Schools
                .Where(s => s.SchoolId == student.SchoolId)
                .Select(s => s.SchoolTypeId)
                .FirstOrDefaultAsync();

            if (schoolLevel.SchoolTypeId != schoolTypeId)
                return result.Failure("School level does not match school type", 400);

            // Validate specialization if provided
            if (studentDto.SpecializationId.HasValue)
            {
                var specialization = await dbContext.Specializations
                    .FirstOrDefaultAsync(s => s.SpecializationId == studentDto.SpecializationId.Value);

                if (specialization == null)
                    return result.Failure("Invalid specialization", 400);

                // Check if school offers this specialization
                var hasSpecialization = await dbContext.Schools
                    .Where(s => s.SchoolId == student.SchoolId)
                    .SelectMany(s => s.Specializations)
                    .AnyAsync(s => s.SpecializationId == studentDto.SpecializationId.Value);

                if (!hasSpecialization)
                    return result.Failure("This specialization is not offered by this school", 400);

                // Check specialization compatibility with school level
                var SchoolLevelId = studentDto.SchoolLevelId;
                var SpecializationId = studentDto.SpecializationId;

                if ((IsFirstYearLycee(SchoolLevelId) && !IsBasicSpecialization(SpecializationId)) ||
                    (IsSecondOrThirdYearLycee(SchoolLevelId) && IsBasicSpecialization(SpecializationId)))
                {
                    return result.Failure("Student Cannot Take This Specialization", 400);
                }
            }

            // Update student information
            student.FirstName = studentDto.FirstName;
            student.LastName = studentDto.LastName;
            student.Address = studentDto.Address;
            student.BirthDate = studentDto.BirthDate;
            student.BirthPlace = studentDto.BirthPlace;
            student.EmergencyContact = studentDto.EmergencyContact;
            student.SchoolLevelId = studentDto.SchoolLevelId;
            student.SpecializationId = studentDto.SpecializationId;
            student.IsActive = studentDto.IsActive;

            await dbContext.SaveChangesAsync();
            return result.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return result.Failure($"Failed to update student: {ex.Message}", 500);
        }
    }

    public async Task<Result<Unit, string>> DeleteStudentAsync(Guid studentId)
    {
        var result = new Result<Unit, string>();

        try
        {
            var student = await dbContext.Students
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
                return result.Failure("Student not found", 404);

            // Instead of hard deletion, mark as inactive
            student.IsActive = false;

            await dbContext.SaveChangesAsync();
            return result.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return result.Failure($"Failed to delete student: {ex.Message}", 500);
        }
    }

    private static List<string> ValidateStudentCsvRecord(StudentCsvRecord record, int schoolTypeId)
    {
        var errors = new List<string>();
        switch (schoolTypeId)
        {
            case (int)SchoolTypeEnum.Lycee when record.SpecializationId == null:
                errors.Add("Specialization ID is required for Lycee students");
                break;
            case (int)SchoolTypeEnum.Moyenne or (int)SchoolTypeEnum.Primaire when record.SpecializationId != null:
                errors.Add("Specialization ID is not allowed for Primaire or Moyenne students");
                break;
        }
        if (record.SpecializationId != null && !int.TryParse(record.SpecializationId, out _))
            errors.Add("Invalid specialization ID format");
        // Required fields validation
        if (string.IsNullOrWhiteSpace(record.StudentFirstName))
            errors.Add("Student first name is required");

        if (string.IsNullOrWhiteSpace(record.StudentLastName))
            errors.Add("Student last name is required");

        if (string.IsNullOrWhiteSpace(record.StudentAddress))
            errors.Add("Student address is required");

        if (string.IsNullOrWhiteSpace(record.StudentBirthDate))
            errors.Add("Student birth date is required");
        else if (!TryParseDate(record.StudentBirthDate, out _))
            errors.Add("Invalid student birth date format");

        if (string.IsNullOrWhiteSpace(record.StudentBirthPlace))
            errors.Add("Student birth place is required");

        if (string.IsNullOrWhiteSpace(record.EmergencyContact))
            errors.Add("Emergency contact is required");

        if (string.IsNullOrWhiteSpace(record.LevelYear))
            errors.Add("School level ID is required");
        else if (!int.TryParse(record.LevelYear, out _))
            errors.Add("Invalid school level ID format");

        // Parent validation
        if (string.IsNullOrWhiteSpace(record.ParentNationalIdNumber))
            errors.Add("Parent national ID is required");

        if (string.IsNullOrWhiteSpace(record.ParentEmail))
            errors.Add("Parent email is required");
        else if (!IsValidEmail(record.ParentEmail))
            errors.Add("Invalid parent email format");

        if (string.IsNullOrWhiteSpace(record.RelationshipToStudentId))
            errors.Add("Relationship to student ID is required");
        else if (!int.TryParse(record.RelationshipToStudentId, out _))
            errors.Add("Invalid relationship ID format");

        if (string.IsNullOrWhiteSpace(record.ParentOccupation))
            errors.Add("Parent occupation is required");

        if (string.IsNullOrWhiteSpace(record.ParentPhoneNumber))
            errors.Add("Parent phone number is required");

        return errors;
    }

    private static DateOnly ParseDateOnly(string dateString)
    {
        if (TryParseDate(dateString, out var date))
            return DateOnly.FromDateTime(date);

        // Fallback to standard parsing
        return DateOnly.FromDateTime(DateTime.Parse(dateString,
            System.Globalization.CultureInfo.InvariantCulture));
    }
    private static int ParseInt(string value)
    {
        return int.Parse(value);
    }
    private static bool TryParseDate(string dateString, out DateTime result)
    {
        // Try specific formats
        string[] formats = {
            "dd-MM-yyyy",
            "dd/MM/yyyy",
            "d/MM/yyyy",
            "d-MM-yyyy",
            "dd-M-yyyy",
            "d-M-yyyy"
        };

        return DateTime.TryParseExact(
            dateString,
            formats,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out result);
    }
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}