using System.Runtime.CompilerServices;
using System.Security.Claims;
using AutoMapper;
using Dirassati_Backend.Common;
using Dirassati_Backend.Data.Enums;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.School.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Dirassati_Backend.Features.School.Services;

public class SchoolServices(
    AppDbContext dbContext,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor) : ISchoolService
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result<GetSchoolInfoDTO, string>> GetSchoolInfosAsync()
    {
        var result = new Result<GetSchoolInfoDTO, string>();
        var schoolId = _httpContextAccessor.HttpContext?.User.FindFirstValue("SchoolId");
        if (schoolId == null)
            return result.Failure("Unauthorized access", 401);

        var school = await _dbContext.Schools.Include(s => s.SchoolType).Include(s => s.AcademicYear).Include(s => s.PhoneNumbers)
            .FirstOrDefaultAsync(s => s.SchoolId.ToString() == schoolId);

        if (school == null)
            return result.Failure("School not found", 404);

        var schoolDTO = _mapper.Map<GetSchoolInfoDTO>(school);
        return result.Success(schoolDTO);
    }


    public async Task<Result<Unit, string>> UpdateSchoolInfos(UpdateSchoolInfosDTO schoolInfosDTO)
    {
        var result = new Result<Unit, string>();

        try
        {
            var schoolId = _httpContextAccessor.HttpContext?.User.FindFirstValue("SchoolId");
            if (schoolId == null)
                return result.Failure("Unauthorized access", 401);

            var school = await _dbContext.Schools
                .Include(s => s.Specializations)
                .FirstOrDefaultAsync(s => s.SchoolId.ToString() == schoolId);

            if (school == null)
                return result.Failure("School not found", 404);

            _mapper.Map(schoolInfosDTO, school);

            if (school.SchoolTypeId == (int)SchoolTypeEnum.Lycee)
            {
                var specializationsToRemove = school.Specializations
                    .Where(s => !schoolInfosDTO.Specializations.Contains(s.SpecializationId))
                    .ToList();

                bool isStudentInSpec = await _dbContext.Students.AnyAsync(s =>
                    s.SchoolId.ToString() == schoolId &&
                    s.Specialization != null &&
                    specializationsToRemove.Contains(s.Specialization));

                if (isStudentInSpec)
                    return result.Failure("Cannot delete specializations that have enrolled students", 400);

                school.Specializations = await _dbContext.Specializations
                    .Where(s => schoolInfosDTO.Specializations.Contains(s.SpecializationId))
                    .ToListAsync();
            }

            await _dbContext.SaveChangesAsync();
            return result.Success(Unit.Value, 204);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating school info: {ex.Message}");
            return result.Failure($"Error updating school info: {ex.Message}", 500);
        }
    }
}