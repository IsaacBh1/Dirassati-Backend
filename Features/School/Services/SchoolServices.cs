using System.Net;
using System.Security.Claims;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Dirassati_Backend.Common;
using Dirassati_Backend.Common.Services;
using Dirassati_Backend.Data.Enums;
using Dirassati_Backend.Features.School.DTOs;
using Dirassati_Backend.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Dirassati_Backend.Features.School.Services;

public class SchoolServices(
    AppDbContext dbContext,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor, IMemoryCache cache, ILogger<SchoolServices> logger, ILogger<CachedServiceBase> logger1) : CachedServiceBase(cache, logger1), ISchoolService
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<Result<GetSchoolInfoDto, string>> GetSchoolInfoAsync()
    {
        var result = new Result<GetSchoolInfoDto, string>();
        var schoolId = _httpContextAccessor.HttpContext?.User.FindFirstValue("SchoolId");
        if (schoolId == null)
            return result.Failure("Unauthorized access", 401);


        if (!Guid.TryParse(schoolId, out var schoolIdGuid))
            return result.Failure($"Invalid SchoolId Format {schoolId}", (int)HttpStatusCode.BadRequest);

        var cacheKey = $"school_info_{schoolIdGuid}";
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            SlidingExpiration = TimeSpan.FromMinutes(30),
            Priority = CacheItemPriority.High
        };

        return await GetOrSetCacheAsync(cacheKey, cacheOptions, async () =>
        {
            var cachingResult = new Result<GetSchoolInfoDto, string>();
            var school = await _dbContext.Schools.Where(s => s.SchoolId == schoolIdGuid).ProjectTo<GetSchoolInfoDto>(_mapper.ConfigurationProvider)
                      .FirstOrDefaultAsync();
            if (school == null)
                return cachingResult.Failure("School not found", 404);
            return cachingResult.Success(school);
        });
    }


    public async Task<Result<Unit, string>> UpdateSchoolInfo(UpdateSchoolInfosDto schoolInfosDTO)
    {
        var result = new Result<Unit, string>();

        try
        {
            logger.LogInformation("Starting school info update process");

            var schoolId = _httpContextAccessor.HttpContext?.User.FindFirstValue("SchoolId");
            if (schoolId == null)
            {
                logger.LogWarning("Unauthorized access attempt - SchoolId not found in claims");
                return result.Failure("Unauthorized access", 401);
            }

            if (!Guid.TryParse(schoolId, out var schoolIdGuid))
            {
                logger.LogWarning("Invalid School Id format: {SchoolId}", schoolId);
                return result.Failure("Invalid School Id", (int)HttpStatusCode.BadRequest);
            }

            logger.LogInformation("Fetching school data for SchoolId: {SchoolId}", schoolIdGuid);
            var school = await _dbContext.Schools
                .Include(s => s.Specializations)
                .Include(s => s.Address)
                .Include(s => s.AcademicYear)
                .Include(s => s.PhoneNumbers)
                .FirstOrDefaultAsync(s => s.SchoolId == schoolIdGuid);

            if (school == null)
            {
                logger.LogWarning("School not found for SchoolId: {SchoolId}", schoolIdGuid);
                return result.Failure("School not found", 404);
            }

            logger.LogInformation("Mapping school info updates for SchoolId: {SchoolId}", schoolIdGuid);
            _mapper.Map(schoolInfosDTO, school);
            _mapper.Map(schoolInfosDTO.Address, school.Address);
            _mapper.Map(schoolInfosDTO.AcademicYear, school.AcademicYear);

            if (school.SchoolTypeId == (int)SchoolTypeEnum.Lycee)
            {
                logger.LogInformation("Processing specializations for Lycee school: {SchoolId}", schoolIdGuid);

                var specializationsToRemove = school.Specializations
                    .Where(s => !schoolInfosDTO.Specializations.Contains(s.SpecializationId))
                    .ToList();

                if (specializationsToRemove.Any())
                {
                    logger.LogInformation("Checking for students in specializations to be removed for SchoolId: {SchoolId}", schoolIdGuid);
                    bool isStudentInSpec = await _dbContext.Students.AnyAsync(s =>
                        s.SchoolId == schoolIdGuid &&
                        s.Specialization != null &&
                        specializationsToRemove.Contains(s.Specialization));

                    if (isStudentInSpec)
                    {
                        logger.LogWarning("Cannot delete specializations with enrolled students for SchoolId: {SchoolId}", schoolIdGuid);
                        return result.Failure("Cannot delete specializations that have enrolled students", 400);
                    }
                }

                school.Specializations = await _dbContext.Specializations
                    .Where(s => schoolInfosDTO.Specializations.Contains(s.SpecializationId))
                    .ToListAsync();
            }

            var cacheKey = $"school_info_{schoolIdGuid}";
            logger.LogInformation("Invalidating cache for key: {CacheKey}", cacheKey);
            InvalidateCache(cacheKey);

            logger.LogInformation("Saving school info changes to database for SchoolId: {SchoolId}", schoolIdGuid);
            await _dbContext.SaveChangesAsync();

            logger.LogInformation("Successfully updated school info for SchoolId: {SchoolId}", schoolIdGuid);
            return result.Success(Unit.Value, 204);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating school info");
            return result.Failure($"Error updating school info: {ex.Message}", 500);
        }
    }
}