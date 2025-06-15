using System.Security.Claims;
using Dirassati_Backend.Common;
using Dirassati_Backend.Common.Dtos;
using Dirassati_Backend.Features.SchoolLevels.DTOs;
using Dirassati_Backend.Features.SchoolLevels.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.SchoolLevels;


[Route("api/[controller]")]
public class LevelsController(SchoolLevelServices schoolLevelServices) : BaseController
{
    [HttpGet]
    [ResponseCache(Duration = 7200, Location = ResponseCacheLocation.Any)]
    [Authorize(Roles = "Employee,Teacher,Parent")]

    public async Task<ActionResult<List<GetSchoolLevelDto>>> GetLevels()
    {
        return await schoolLevelServices.GetAllLevelsAsync();
    }


    [HttpGet("specializations")]
    [Authorize(Roles = "Employee,Teacher,Parent")]

    [ResponseCache(Duration = 7200, Location = ResponseCacheLocation.Any)]

    public async Task<ActionResult<List<SpecializationDto>>> GetAllSpecializations()
    {
        return await schoolLevelServices.GetAllSpecializationsAsync();
    }


    [HttpGet("specializations/school")]
    [Authorize(Roles = "Employee,Teacher,Parent")]


    public async Task<ActionResult<List<SpecializationDto>>> GetSchoolSpecializations()
    {
        var schoolId = User.FindFirstValue("SchoolId");
        if (schoolId is null)
            return BadRequest("Missing SchoolId in token");
        var result = await schoolLevelServices.GetSchoolSpecializations(schoolId);
        return HandleResult(result);

    }


    [HttpPut("specializations/add")]
    [Authorize(Roles = "Employee")]


    public async Task<ActionResult> EditSpecialization(EditSpecializationDto specializationDTO)
    {
        var result = await schoolLevelServices.EditSchoolSpecializations(User.FindFirstValue("SchoolId")!, specializationDTO.SpecializationIds);
        return HandleResult(result);

    }



}
