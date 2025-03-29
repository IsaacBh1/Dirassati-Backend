// TeacherController.cs
using System.Security.Claims;
using Dirassati_Backend.Features.Teachers.Dtos;
using Dirassati_Backend.Features.Teachers.Services;
using Dirassati_Backend.Hubs.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Dirassati_Backend.Features.Teachers.Controllers;

[ApiController]
[Authorize]
[Route("api/teacher")]
public class TeacherController : BaseController
{
    private readonly TeacherServices _teacherServices;
    private readonly IHubContext<ParentNotificationHub, IParentClient> _hubContext;
    public TeacherController(TeacherServices teacherServices, IHubContext<ParentNotificationHub, IParentClient> hubContext)
    {
        _teacherServices = teacherServices;
        _hubContext = hubContext;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateTeacher([FromBody] TeacherInfosDTO teacherDto)
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
    public async Task<ActionResult<GetTeacherInfosDTO>> GetTeacherInfo(string id)
    {
        var schoolId = User.FindFirstValue("SchoolId");
        if (schoolId is null)
            return Unauthorized();
        return HandleResult(await _teacherServices.GetTeacherInfos(id, schoolId));
    }

    [HttpGet]
    public async Task<ActionResult<List<GetTeacherInfosDTO>>> GetTeachers()
    {
        var schoolId = User.FindFirstValue("SchoolId");
        if (schoolId is null)
            return Unauthorized();
        return HandleResult(await _teacherServices.GetTeachers(schoolId));
    }

    [HttpGet("contract-types")]
    public async Task<ActionResult<List<ContractTypeDTO>>> GetContractTypes()
    {
        return HandleResult(await _teacherServices.GetContractTypes());
    }


    [HttpPost("/reports/add")]
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
        catch (System.Exception e)
        {
            Console.WriteLine(e);
            return Problem(e.Message, statusCode: 500);

        }

    }
}