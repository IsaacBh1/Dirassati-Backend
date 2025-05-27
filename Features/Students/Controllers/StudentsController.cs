using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using Dirassati_Backend.Common;
using Dirassati_Backend.Common.Dtos;
using Dirassati_Backend.Features.Auth.Register.Dtos;
using Dirassati_Backend.Features.Common;
using Dirassati_Backend.Features.Students.DTOs;
using Dirassati_Backend.Features.Students.Models;
using Dirassati_Backend.Features.Students.Repositories;
using Dirassati_Backend.Features.Students.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.Students;

[Route("api/students")]
[ApiController]
public class StudentsController(StudentServices studentServices, IStudentRepository studentRepository) : BaseController
{
    private readonly StudentServices _studentServices = studentServices;
    private readonly IStudentRepository _studentRepository = studentRepository;

    /// <summary>
    /// Get a student by ID
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <returns>Student details</returns>
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentById(Guid id)
    {
        var student = await _studentRepository.GetStudentByIdAsync(id);
        if (student == null)
            return NotFound($"Student with ID {id} not found.");

        return Ok(student);
    }

    /// <summary>
    /// Add a new student
    /// </summary>
    /// <param name="studentDTO">Student information</param>
    /// <returns>Success or error message</returns>
    [Authorize]
    [HttpPost("add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> AddStudent(AddStudentDto studentDTO)
    {
        var schoolId = User.FindFirstValue("SchoolId")!;
        var result = await _studentServices.AddStudentAsync(schoolId, studentDTO);
        return HandleResult(result);
    }

    /// <summary>
    /// Update a student's information
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <param name="studentDto">Updated student information</param>
    /// <returns>Success or error message</returns>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateStudent(Guid id, UpdateStudentDto studentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _studentServices.UpdateStudentAsync(id, studentDto);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete a student (marks as inactive)
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <returns>Success or error message</returns>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteStudent(Guid id)
    {
        var result = await _studentServices.DeleteStudentAsync(id);
        return HandleResult(result);
    }

    [HttpGet("list")]
    [Authorize]
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
    [HttpPost("import-csv")]
    [Authorize]
    [ProducesResponseType(typeof(StudentImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportStudentsFromCsv([FromForm] StudentCsvUploadModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validate file
        if (model.CsvFile.Length == 0)
        {
            return BadRequest("File is empty");
        }

        // Check file extension
        var extension = Path.GetExtension(model.CsvFile.FileName).ToLowerInvariant();
        if (extension != ".csv")
        {
            return BadRequest("Only CSV files are allowed");
        }

        // Verify file content type
        if (!model.CsvFile.ContentType.Equals("text/csv") &&
            !model.CsvFile.ContentType.Equals("application/csv") &&
            !model.CsvFile.ContentType.Equals("application/vnd.ms-excel"))
        {
            return BadRequest("File content type is not valid. Please upload a valid CSV file.");
        }

        var schoolId = User.FindFirstValue("SchoolId");
        if (string.IsNullOrEmpty(schoolId))
        {
            return Unauthorized("Invalid or missing School ID claim.");
        }

        var result = await _studentServices.ImportStudentsFromCsvAsync(schoolId, model.CsvFile, model.HasHeaders);
        return HandleResult(result);
    }

    [HttpGet("import-template")]
    [Authorize]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public IActionResult GetImportTemplate()
    {
        try
        {
            // Create a memory stream to write the CSV to
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, new UTF8Encoding(true));
            using var csv = new CsvHelper.CsvWriter(writer, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture));

            // Register class map to use column names
            csv.Context.RegisterClassMap<StudentCsvRecordMap>();

            // Write header row only
            csv.WriteHeader<StudentCsvRecord>();
            csv.NextRecord();

            // No records to write, just the header

            // Make sure everything is written
            writer.Flush();

            // Get the bytes from the memory stream
            var bytes = memoryStream.ToArray();

            // Return as a file
            return File(bytes, "text/csv", "student-import-template.csv");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error generating template: {ex.Message}");
        }
    }

    /// <summary>
    /// Get the school associated with a student by student ID
    /// </summary>
    /// <param name="id">Student ID</param>
    /// <returns>School details</returns>
    [HttpGet("{id}/school")]
    [ProducesResponseType(typeof(SchoolDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSchoolByStudentId(Guid id)
    {
        var student = await _studentRepository.GetStudentByIdAsync(id);
        if (student == null)
        {
            return NotFound($"Student with ID {id} not found.");
        }

        var school = await _studentRepository.GetSchoolByStudentIdAsync(id);
        if (school == null)
        {
            return NotFound($"School for student with ID {id} not found.");
        }

        var schoolDto = new SchoolDto
        {
            Name = school.Name,
            Address = new AddressDto
            {
                Street = school.Address?.Street ?? string.Empty,
                City = school.Address?.City ?? string.Empty,
                State = school.Address?.State ?? string.Empty,
                PostalCode = school.Address?.PostalCode ?? string.Empty,
                Country = school.Address?.Country ?? string.Empty
            },
            PhoneNumber = school.PhoneNumbers?.FirstOrDefault()?.ToString() ?? string.Empty,
            SchoolTypeId = school.SchoolTypeId,
            Email = school.Email,
            LogoUrl = school.LogoUrl,
            WebsiteUrl = school.WebsiteUrl,
            AcademicYear = new AcademicYearDto
            {
                StartDate = school.AcademicYear.StartDate,
                EndDate = school.AcademicYear.EndDate
            },
        };

        return Ok(schoolDto);
    }

}

