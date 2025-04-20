using Dirassati_Backend.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Dirassati_Backend.Features.Notes.Repos;
using Dirassati_Backend.Features.Notes.Dtos;
using Microsoft.AspNetCore.Authorization;
using Dirassati_Backend.Features.Students.Repositories;
using Dirassati_Backend.Domain.Services;

using Microsoft.EntityFrameworkCore;
using Dirassati_Backend.Data.DTOs;
using System.Text;
using System.Globalization;
using CsvHelper;
using System.Security.Claims;
using Dirassati_Backend.Persistence;

namespace Dirassati_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotesController : ControllerBase
    {
        private readonly INoteRepository _noteRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ICsvService _csvService;
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotesController(
            INoteRepository noteRepository,
            IStudentRepository studentRepository,
            ICsvService csvService,
            IHttpContextAccessor httpContextAccessor,
            AppDbContext context)
        {
            _noteRepository = noteRepository;
            _studentRepository = studentRepository;
            _csvService = csvService;
            _httpContextAccessor = httpContextAccessor;
            _context = context;

        }

        [HttpPost]
        public async Task<ActionResult<Note>> AddNote(CreateNoteDto createNoteDto)
        {
            var schoolIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("SchoolId");
            if (schoolIdClaim == null || !Guid.TryParse(schoolIdClaim.Value, out Guid schoolId))
            {
                return Unauthorized("Invalid School ID in token.");
            }

            var note = new Note
            {
                StudentId = createNoteDto.StudentId,
                Tremester = createNoteDto.Tremester,
                TeacherId = createNoteDto.TeacherId,
                AcademicYearId = createNoteDto.AcademicYearId,
                ExamTypeId = createNoteDto.ExamTypeId,
                SubjectId = createNoteDto.SubjectId,
                Value = createNoteDto.Value,
                GroupId = createNoteDto.GroupId,
                SchoolId = schoolId
            };

            var addedNote = await _noteRepository.AddNoteAsync(note);
            return CreatedAtAction(nameof(AddNote), new { id = addedNote.NoteId }, addedNote);
        }

        [HttpGet("template")]
        public async Task<IActionResult> GetCsvTemplate([FromQuery] Guid groupId)
        {
            try
            {
                var schoolId = GetSchoolIdFromToken();

                // Verify group exists in school
                var groupExists = await _context.Groups
                    .AnyAsync(g => g.GroupId == groupId && g.SchoolId == schoolId);

                if (!groupExists)
                    return BadRequest("Invalid group ID for this school");

                var students = await _studentRepository.GetStudentsByGroupAsync(groupId);

                if (!students.Any())
                    return NotFound("No students found in this group");

                var records = students.Select(s => new CsvNoteRecord
                {
                    StudentId = s.StudentId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Value = null
                }).ToList();

                return GenerateCsvFile(records, $"NotesTemplate-Group-{groupId}.csv");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating template: {ex.Message}");
            }
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreateNotes([FromForm] BulkNoteCreateDto dto)
        {
            try
            {
                var schoolId = GetSchoolIdFromToken();
                var teacherId = GetTeacherIdFromToken();

                var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t =>
                    t.TeacherId == teacherId &&
                    t.SchoolId == schoolId);

                if (teacher == null)
                    return Unauthorized("Teacher not found in this school");

                var group = await _context.Groups
                    .FirstOrDefaultAsync(g => g.GroupId == dto.GroupId && g.SchoolId == schoolId);

                if (group == null)
                    return BadRequest("Invalid group ID for this school");

                var students = await _studentRepository.GetStudentsByGroupAsync(dto.GroupId);
                var csvRecords = await _csvService.ProcessNotesCsv(dto.CsvFile);

                var notes = new List<Note>();
                var errors = new List<string>();

                foreach (var record in csvRecords)
                {
                    var validationResult = ValidateRecord(record, students, dto.GroupId);
                    if (!validationResult.IsValid)
                    {
                        errors.Add(validationResult.ErrorMessage);
                        continue;
                    }

                    notes.Add(CreateNoteEntity(record, dto, schoolId, teacherId));
                }

                if (errors.Any())
                    return BadRequest(new { Errors = errors });

                await _noteRepository.BulkAddAsync(notes);
                return Ok(new { Message = $"{notes.Count} notes created for group {dto.GroupId}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing notes: {ex.Message}");
            }
        }

        private (bool IsValid, string ErrorMessage) ValidateRecord(CsvNoteRecord record, List<Student> students, Guid groupId)
        {
            if (!record.Value.HasValue)
                return (false, $"Missing value for {record.FirstName} {record.LastName}");

            var student = students.FirstOrDefault(s =>
                s.StudentId == record.StudentId &&
                s.FirstName == record.FirstName &&
                s.LastName == record.LastName);

            if (student == null)
                return (false, $"Student {record.FirstName} {record.LastName} not found in group");

            if (student.GroupId != groupId)
                return (false, $"Student {record.FirstName} {record.LastName} doesn't belong to this group");

            if (record.Value < 0 || record.Value > 20)
                return (false, $"Invalid value for {record.FirstName} {record.LastName}. Must be between 0-20");

            return (true, string.Empty);
        }

        private Note CreateNoteEntity(CsvNoteRecord record, BulkNoteCreateDto dto, Guid schoolId, Guid teacherId)
        {
            return new Note
            {
                StudentId = record.StudentId,
                Value = record.Value ?? 0,
                Tremester = dto.Tremester,
                AcademicYearId = dto.AcademicYearId,
                ExamTypeId = dto.ExamTypeId,
                SubjectId = dto.SubjectId,
                GroupId = dto.GroupId,
                SchoolId = schoolId,
                TeacherId = teacherId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private FileContentResult GenerateCsvFile(List<CsvNoteRecord> records, string fileName)
        {
            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.Context.TypeConverterOptionsCache.GetOptions<double?>().NullValues.Add(string.Empty);
                csv.WriteRecords(records);
            }
            return File(memoryStream.ToArray(), "text/csv", fileName);
        }

        private Guid GetSchoolIdFromToken()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("SchoolId");
            if (Guid.TryParse(claim?.Value, out var schoolId))
                return schoolId;
            throw new UnauthorizedAccessException("Invalid School ID in token");
        }

        private Guid GetTeacherIdFromToken()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst("TeacherId");
            if (Guid.TryParse(claim?.Value, out var teacherId))
                return teacherId;
            throw new UnauthorizedAccessException("Invalid Teacher ID in token");
        }
    }

}
