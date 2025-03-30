using System.Security.Claims;
using Dirassati_Backend.Features.Teachers.Dtos;
using Dirassati_Backend.Features.Teachers.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.Teachers.Controllers;

[ApiController]
[Authorize]
[Route("api/teacher")]
public class TeacherController : BaseController
{
    private readonly TeacherServices _teacherServices;

    public TeacherController(TeacherServices teacherServices)
    {
        _teacherServices = teacherServices;
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
}