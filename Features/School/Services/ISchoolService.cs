using Dirassati_Backend.Common;
using Dirassati_Backend.Features.School.DTOs;

namespace Dirassati_Backend.Features.School.Services;

public interface ISchoolService
{
    public Task<Result<GetSchoolInfoDto, string>> GetSchoolInfosAsync();
    public Task<Result<Unit, string>> UpdateSchoolInfos(UpdateSchoolInfosDto schoolInfosDTO);
}
