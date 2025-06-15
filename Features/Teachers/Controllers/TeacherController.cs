using System.Security.Claims;
using Dirassati_Backend.Common;
using Dirassati_Backend.Features.Teachers.Dtos;
using Dirassati_Backend.Features.Teachers.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.Teachers.Controllers;

[ApiController]
[Authorize]
[Route("api/teacher")]
public class TeacherController(TeacherServices teacherServices) : BaseController
{
    private readonly TeacherServices _teacherServices = teacherServices;

    [HttpPost("create")]
    public async Task<IActionResult> CreateTeacher([FromBody] TeacherInfosDto teacherDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var schoolId = User.FindFirstValue("SchoolId");
            if (schoolId == null)
                return Unauthorized();
            var teacherId = await _teacherServices.RegisterTeacherAsync(teacherDto, schoolId);
            return CreatedAtAction("GetTeacherInfo", new { id = teacherId }, null);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}", Name = "GetTeacherInfo")]
    public async Task<ActionResult<GetTeacherInfosDto>> GetTeacherInfo(string id)
    {
        var schoolId = User.FindFirstValue("SchoolId");
        if (schoolId is null)
            return Unauthorized();
        return HandleResult(await _teacherServices.GetTeacherInfos(id, schoolId));
    }

    [HttpGet]
    public async Task<ActionResult<List<GetTeacherInfosDto>>> GetTeachers()
    {
        var schoolId = User.FindFirstValue("SchoolId");
        if (schoolId is null)
            return Unauthorized();
        return HandleResult(await _teacherServices.GetTeachers(schoolId));
    }

    [HttpGet("contract-types")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult<List<ContractTypeDto>>> GetContractTypes()
    {
        return HandleResult(await _teacherServices.GetContractTypes());
    }


    /// <summary>
    /// Add teacher report
    /// </summary>
    /// <param name="reportDto"></param>
    /// <returns></returns>

    [HttpPost("reports/add")]
    public async Task<ActionResult<GetStudentReportDto>> AddTeacherReport([FromBody] AddStudentReportDto reportDto)
    {

        try
        {
            var teacherId = User.FindFirstValue("TeacherId");
            var result = await _teacherServices.AddTeacherReportAsync(teacherId, reportDto);
            if (result.IsSuccess && result.Value != null)
            {

                await _teacherServices.TriggerSendReportNotification(result.Value);
            }
            return HandleResult(result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Problem(e.Message, statusCode: 500);

        }

    }
}