using System.Security.Claims;
using Dirassati_Backend.Features.Students.DTOs;
using Dirassati_Backend.Features.Students.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.Students;

[Authorize]
[Route("api/students")] // add them later 

public class StudentsController(StudentServices studentServices) : BaseController
{
    private readonly StudentServices _studentServices = studentServices;

    [HttpPost("add")]
    public async Task<ActionResult> AddStudent(AddStudentDTO studentDTO)
    {
        //in case of success resturs the studentId
        var schoolId = User.FindFirstValue("SchoolId")!;
        var result = await _studentServices.AddStudentAsync(schoolId, studentDTO);
        return HandleResult(result);
    }
}

