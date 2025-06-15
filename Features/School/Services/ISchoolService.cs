using Dirassati_Backend.Common;
using Dirassati_Backend.Features.School.DTOs;

namespace Dirassati_Backend.Features.School.Services;

public interface ISchoolService
{
    public Task<Result<GetSchoolInfoDto, string>> GetSchoolInfoAsync();
    public Task<Result<Unit, string>> UpdateSchoolInfo(UpdateSchoolInfosDto schoolInfosDTO);
}
