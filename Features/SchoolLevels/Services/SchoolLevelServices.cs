using Dirassati_Backend.Common;
using Dirassati_Backend.Common.Dtos;
using Dirassati_Backend.Data.Enums;
using Dirassati_Backend.Features.SchoolLevels.DTOs;
using Dirassati_Backend.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Dirassati_Backend.Features.SchoolLevels.Services;

public class SchoolLevelServices(AppDbContext dbContext)
{
    public async Task<List<GetSchoolLevelDto>> GetAllLevelsAsync()
    {
        var levels = await dbContext.SchoolLevels.Include(sl => sl.SchoolType).ToListAsync();
        return levels.Select(lev => new GetSchoolLevelDto { LevelId = lev.LevelId, SchoolTypeId = lev.SchoolTypeId, LevelYear = lev.LevelYear, SchoolTypeName = lev.SchoolType.Name }).ToList();

    }

    public async Task<List<SpecializationDto>> GetAllSpecializationsAsync()
    {
        return await dbContext.Specializations.Select(s => new SpecializationDto { Name = s.Name, SpecializationId = s.SpecializationId }).ToListAsync();
    }

    public async Task<Result<List<SpecializationDto>, string>> GetSchoolSpecializations(string schoolId)
    {
        var result = new Result<List<SpecializationDto>, string>();

        if (!Guid.TryParse(schoolId, out var schoolIdGuid))
            return result.Failure("Invalid School Id", (int)HttpStatusCode.BadRequest);
        var school = await dbContext.Schools
            .Include(s => s.SchoolType)

            .FirstOrDefaultAsync(s => s.SchoolId == schoolIdGuid);

        if (school is null)
            return result.Failure("School Not Found", 404);

        if (school.SchoolTypeId != (int)SchoolTypeEnum.Lycee)
            return result.Failure("School Is Not High School", 400);

        await dbContext.Entry(school)
        .Collection(s => s.Specializations)
        .LoadAsync();
        var specs = school.Specializations.Select(s => new SpecializationDto { Name = s.Name, SpecializationId = s.SpecializationId }).ToList();

        return result.Success(specs);
    }

    public async Task<Result<Unit, string>> EditSchoolSpecializations(
        string schoolId,
        List<int> specializationIds)
    {
        var result = new Result<Unit, string>();
        if (!Guid.TryParse(schoolId, out var schoolIdGuid))
            return result.Failure("Invalid School Id", (int)HttpStatusCode.BadRequest);
        var school = await dbContext.Schools
            .Include(s => s.Specializations)
            .FirstOrDefaultAsync(s => s.SchoolId == schoolIdGuid);

        if (school == null)
            return result.Failure("School Not Found", 404);

        if (school.SchoolTypeId != (int)SchoolTypeEnum.Lycee)
            return result.Failure("School Is Not High School", 400);

        var specializations = await dbContext.Specializations
            .Where(s => specializationIds.Contains(s.SpecializationId))
            .ToListAsync();

        if (specializations.Count != specializationIds.Count)
            return result.Failure("One or more specialization IDs are invalid", 400);

        school.Specializations.Clear();
        foreach (var spec in specializations)
        {
            school.Specializations.Add(spec);
        }

        await dbContext.SaveChangesAsync();
        return result.Success(Unit.Value, (int)HttpStatusCode.NoContent);
    }

}