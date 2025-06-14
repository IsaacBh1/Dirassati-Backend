using System.Globalization;
using System.Text;
using CsvHelper;
using Dirassati_Backend.Common;
using Dirassati_Backend.Data.DTOs;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Domain.Services;
using Dirassati_Backend.Features.Notes.Dtos;
using Dirassati_Backend.Features.Notes.Repos;
using Dirassati_Backend.Features.Notes.Services;
using Dirassati_Backend.Features.Students.Repositories;
using Dirassati_Backend.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Features.Notes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotesController(
        INoteRepository noteRepository,
        IStudentRepository studentRepository,
        ICsvService csvService,
        IHttpContextAccessor httpContextAccessor,
        AppDbContext context,
        INotesServices notesServices)
        : BaseController
    {
        private readonly INotesServices _notesServices = notesServices;

        [HttpPost]
        [Authorize(Roles = "Employee,Teacher")]

        public async Task<ActionResult<Note>> AddNote(CreateNoteDto createNoteDto)
        {
            var schoolIdClaim = httpContextAccessor.HttpContext?.User.FindFirst("SchoolId");
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

            var addedNote = await noteRepository.AddNoteAsync(note);
            return CreatedAtAction(nameof(AddNote), new { id = addedNote.NoteId }, addedNote);
        }

        [HttpGet("template")]
        [Authorize(Roles = "Employee,Teacher")]

        public async Task<IActionResult> GetCsvTemplate([FromQuery] Guid groupId)
        {
            try
            {
                var schoolId = GetSchoolIdFromToken();

                var groupExists = await context.Groups
                    .AnyAsync(g => g.GroupId == groupId && g.SchoolId == schoolId);

                if (!groupExists)
                    return BadRequest("Invalid group ID for this school");

                var students = await studentRepository.GetStudentsByGroupAsync(groupId);

                if (students.Count == 0)
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
        [Authorize(Roles = "Employee,Teacher")]

        public async Task<IActionResult> BulkCreateNotes([FromForm] BulkNoteCreateDto dto)
        {
            try
            {
                var schoolId = GetSchoolIdFromToken();
                var teacherId = GetTeacherIdFromToken();

                // Validate Teacher
                var teacher = await context.Teachers
                    .FirstOrDefaultAsync(t => t.TeacherId == teacherId && t.SchoolId == schoolId);
                if (teacher == null)
                    return Unauthorized("Teacher not found in this school");

                // Validate Group
                var group = await context.Groups
                    .FirstOrDefaultAsync(g => g.GroupId == dto.GroupId && g.SchoolId == schoolId);
                if (group == null)
                    return BadRequest("Invalid group ID for this school");


                // Validate ExamType
                var examType = await context.ExamTypes
                    .FirstOrDefaultAsync(et => et.ExamTypeId == dto.ExamTypeId);
                if (examType == null)
                    return BadRequest("Invalid ExamTypeId");

                // Validate Subject
                var subject = await context.Subjects
                    .FirstOrDefaultAsync(s => s.SubjectId == dto.SubjectId);
                if (subject == null)
                    return BadRequest("Invalid SubjectId");

                // Validate Tremester
                if (dto.Tremester < 1 || dto.Tremester > 3)
                    return BadRequest("Tremester must be between 1 and 3");

                var students = await studentRepository.GetStudentsByGroupAsync(dto.GroupId);
                var csvRecords = await csvService.ProcessNotesCsv(dto.CsvFile);

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

                    var note = CreateNoteEntity(record, dto, schoolId, teacherId);
                    // Log note details for debugging
                    Console.WriteLine($"Inserting Note: AcademicYearId={note.AcademicYearId}, ExamTypeId={note.ExamTypeId}, GroupId={note.GroupId}, SchoolId={note.SchoolId}, StudentId={note.StudentId}, SubjectId={note.SubjectId}, TeacherId={note.TeacherId}, Tremester={note.Tremester}, Value={note.Value}");
                    notes.Add(note);
                }

                if (errors.Count != 0)
                    return BadRequest(new { Errors = errors });

                await noteRepository.BulkAddAsync(notes);
                return Ok(new { Message = $"{notes.Count} notes created for group {dto.GroupId}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing notes: {ex.Message}");
            }
        }

        private static (bool IsValid, string ErrorMessage) ValidateRecord(CsvNoteRecord record, List<Student> students, Guid groupId)
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

        private static Note CreateNoteEntity(CsvNoteRecord record, BulkNoteCreateDto dto, Guid schoolId, Guid teacherId)
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
            var claim = httpContextAccessor.HttpContext?.User.FindFirst("SchoolId");
            if (Guid.TryParse(claim?.Value, out var schoolId))
                return schoolId;
            throw new UnauthorizedAccessException("Invalid School ID in token");
        }

        private Guid GetTeacherIdFromToken()
        {
            var claim = httpContextAccessor.HttpContext?.User.FindFirst("TeacherId");
            if (Guid.TryParse(claim?.Value, out var teacherId))
                return teacherId;
            throw new UnauthorizedAccessException("Invalid Teacher ID in token");
        }

        [HttpGet]
        [Authorize(Roles = "Parent")]

        public async Task<ActionResult<List<StudentNotesResponseDto>>> GetAllChildrenNotes()
        {
            var parentIdClaim = httpContextAccessor.HttpContext?.User.FindFirst("ParentId");
            if (parentIdClaim == null || !Guid.TryParse(parentIdClaim.Value, out Guid parentId))
            {
                return Unauthorized("Invalid Parent ID in token.");
            }

            var result = await _notesServices.GetStudentNotesByParentIdAsync(parentId);
            return HandleResult(result);
        }

        [HttpGet("{studentId}")]
        [Authorize(Roles = "Parent")]

        public async Task<ActionResult<StudentNotesResponseDto>> GetStudentNotes(Guid studentId)
        {
            var parentIdClaim = httpContextAccessor.HttpContext?.User.FindFirst("ParentId");
            if (parentIdClaim == null || !Guid.TryParse(parentIdClaim.Value, out Guid parentId))
            {
                return Unauthorized("Invalid Parent ID in token.");
            }

            var result = await _notesServices.GetStudentNotesByStudentIdForParentAsync(parentId, studentId);
            return HandleResult(result);
        }
    }
}