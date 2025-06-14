using Dirassati_Backend.Common;
using Dirassati_Backend.Features.School.DTOs;
using Dirassati_Backend.Features.School.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.School;

[Route("api/[controller]")]
public class SchoolController(ISchoolService schoolService) : BaseController
{
    private readonly ISchoolService _schoolService = schoolService;

    [HttpGet]
    [Authorize(Roles = "Employee,Teacher,Parent")]

    public async Task<ActionResult<GetSchoolInfoDto>> GetSchoolInfo()
    {
        var result = await _schoolService.GetSchoolInfosAsync();
        return HandleResult(result);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> UpdateSchoolInfos(UpdateSchoolInfosDto schoolInfosDTO)
    {
        var result = await _schoolService.UpdateSchoolInfos(schoolInfosDTO);
        return HandleResult(result);
    }



}
