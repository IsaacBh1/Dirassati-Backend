using Dirassati_Backend.Features.School.DTOs;
using Dirassati_Backend.Features.School.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.School;

[Route("api/[controller]")]

public class SchoolController(ISchoolService schoolService) : BaseController
{
    private readonly ISchoolService _schoolService = schoolService;

    [HttpGet]
    [ProducesResponseType(typeof(GetSchoolInfoDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetSchoolInfoDTO>> GetSchoolInfo()
    {
        var result = await _schoolService.GetSchoolInfosAsync();
        return HandleResult(result);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]

    public async Task<IActionResult> UpdateSchoolInfos(UpdateSchoolInfosDTO schoolInfosDTO)
    {
        var result = await _schoolService.UpdateSchoolInfos(schoolInfosDTO);
        return HandleResult(result);
    }

}
