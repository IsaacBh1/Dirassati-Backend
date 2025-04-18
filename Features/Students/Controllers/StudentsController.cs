using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Dirassati_Backend.Features.Common;
using Dirassati_Backend.Features.Students.DTOs;
using Dirassati_Backend.Features.Students.Repositories;
using Dirassati_Backend.Features.Students.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.Students;

[Authorize]
[Route("api/students")]

public class StudentsController(StudentServices studentServices, IStudentRepository studentRepository) : BaseController
{
    private readonly StudentServices _studentServices = studentServices;
    private readonly IStudentRepository _studentRepository = studentRepository;

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StudentDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetStudentById(Guid id)
    {
        var student = await _studentRepository.GetStudentByIdAsync(id);
        if (student == null)
            return NotFound($"Student with ID {id} not found.");

        return Ok(student);
    }



    [HttpPost("add")]
    public async Task<ActionResult> AddStudent(AddStudentDTO studentDTO)
    {
        var schoolId = User.FindFirstValue("SchoolId")!;
        var result = await _studentServices.AddStudentAsync(schoolId, studentDTO);
        return HandleResult(result);
    }


    [HttpGet("list")]
    [ProducesResponseType(typeof(PaginatedResult<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStudentsBySchoolId(
    [FromQuery][Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")] int page = 1,
    [FromQuery][Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")] int pageSize = 10)
    {
        var schoolIdClaim = User.FindFirstValue("SchoolId");
        if (string.IsNullOrEmpty(schoolIdClaim) || !Guid.TryParse(schoolIdClaim, out var schoolId))
        {
            return Unauthorized("Invalid or missing School ID claim.");
        }

        if (!await _studentRepository.SchoolExistsAsync(schoolId))
        {
            return NotFound($"School with ID {schoolId} not found.");
        }

        var result = await _studentRepository.GetStudentsBySchoolIdAsync(schoolId, page, pageSize);
        return Ok(result);

    }


}

