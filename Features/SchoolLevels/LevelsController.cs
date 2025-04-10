using System.Security.Claims;
using Dirassati_Backend.Common.Dtos;
using Dirassati_Backend.Features.SchoolLevels.DTOs;
using Dirassati_Backend.Features.SchoolLevels.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.SchoolLevels;
[Route("api/[controller]")]
public class LevelsController(SchoolLevelServices schoolLevelServices) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<List<GetSchoolLevelDTO>>> GetLevels()
    {
        return await schoolLevelServices.GetAllLevelsAsync();
    }


    [HttpGet("specializations")]
    public async Task<ActionResult<List<SpecializationDto>>> GetAllSpecializations()
    {
        return await schoolLevelServices.GetAllSpecializationsAsync();
    }


    [HttpGet("specializations/school")]

    public async Task<ActionResult<List<SpecializationDto>>> GetSchoolSpecializations()
    {
        var schoolId = User.FindFirstValue("SchoolId");
        if (schoolId is null)
            return BadRequest("Missing SchoolId in token");
        var result = await schoolLevelServices.GetSchoolSpecializations(schoolId);
        return HandleResult(result);

    }


    [HttpPut("specializations/add")]

    public async Task<ActionResult> EditSpecialization(EditSpecializationDTO specializationDTO)
    {
        var result = await schoolLevelServices.EditSchoolSpecializations(User.FindFirstValue("SchoolId")!, specializationDTO.SpecializationIds);
        return HandleResult(result);

    }



}
