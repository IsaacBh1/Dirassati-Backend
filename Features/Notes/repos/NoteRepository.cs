using Dirassati_Backend.Data.Models;
using Microsoft.EntityFrameworkCore;
using Dirassati_Backend.Features.Notes.Dtos;
using Dirassati_Backend.Persistence;

namespace Dirassati_Backend.Features.Notes.Repos;

public class NoteRepository(AppDbContext context) : INoteRepository
{
    private readonly AppDbContext _context = context;

    public async Task<Note> AddNoteAsync(Note note)
    {
        await _context.Notes.AddAsync(note);
        await _context.SaveChangesAsync();
        return note;
    }

    public async Task BulkAddAsync(IEnumerable<Note> notes)
    {
        await _context.Notes.AddRangeAsync(notes);
        await _context.SaveChangesAsync();

    }
    public async Task<List<StudentNotesResponseDto>> GetStudentNotesByParentIdAsync(Guid parentId)
    {
        var studentsIds = await _context.Students
            .Where(s => s.ParentId == parentId)
            .Select(s => s.StudentId).ToListAsync();


        var result = new List<StudentNotesResponseDto>();

        foreach (var Id in studentsIds)
        {
            var student = await _context.Students
                .Include(s => s.Group)
                .Where(s => s.StudentId == Id)
                .Select(s => new
                {
                    Student = s,
                    SchoolLevel = s.SchoolLevel.LevelYear,
                    GroupName = s.Group != null ? s.Group.GroupName : "Not assigned"
                }).FirstOrDefaultAsync();

            var notes = await GetStudentNotesByStudentId(Id);

            result.Add(new StudentNotesResponseDto()
            {
                StudentName = $"{student!.Student.FirstName} {student.Student.LastName}",
                SchoolLevel = student.SchoolLevel,
                GroupName = student.GroupName,
                Notes = notes
            });
        }

        return result;
    }

    public async Task<List<StudentNoteDto>> GetStudentNotesByStudentId(Guid Id)
    {
        var notes = await _context.Notes
            .Include(n => _context.Subjects)
            .Include(n => n.ExamType)
            .Include(n => n.Teacher)
            .ThenInclude(t => t!.User)
            .Where(n => n.StudentId == Id)
            .Select(n => new StudentNoteDto()
            {
                NoteId = n.NoteId,
                Value = n.Value,
                Tremester = n.Tremester,
                SubjectName = n.Subject == null ? "Not assigned" : n.Subject.Name,
                ExamTypeName = n.ExamType == null ? "Not assigned" : n.ExamType.Name,
                TeacherName = n.Teacher != null ? n.Teacher.User.FirstName + " " + n.Teacher.User.LastName : "Not assigned",
                CreatedAt = n.CreatedAt
            })
            .OrderByDescending(n => n.CreatedAt)
            .ThenBy(n => n.SubjectName)
            .ToListAsync();
        return notes;
    }
    public async Task<bool> CheckStudentBelongsToParentAsync(Guid parentId, Guid studentId)
    {
        return await _context.Students
            .AnyAsync(s => s.StudentId == studentId && s.ParentId == parentId);
    }

    public async Task<SimpleStudentDetailsDto> GetStudentDetailsAsync(Guid studentId)
    {
        var student = await _context.Students
            .Include(s => s.SchoolLevel)
            .Include(s => s.Group)
            .Where(s => s.StudentId == studentId)
            .Select(s => new SimpleStudentDetailsDto
            {
                StudentName = $"{s.FirstName} {s.LastName}",
                SchoolLevel = s.SchoolLevel.LevelYear,
                GroupName = s.Group != null ? s.Group.GroupName : "Not assigned"
            })
            .FirstOrDefaultAsync();

        return student ?? new SimpleStudentDetailsDto
        {
            StudentName = "Unknown",
            SchoolLevel = 0,
            GroupName = "Unknown"
        };
    }
}