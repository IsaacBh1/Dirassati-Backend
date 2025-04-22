using System.Net;
using Dirassati_Backend.Common;
using Dirassati_Backend.Features.Notes.Dtos;
using Dirassati_Backend.Features.Notes.Repos;

namespace Dirassati_Backend.Features.Notes.Services;

public class NoteService:INotesServices
{
    private readonly INoteRepository _noteRepository;

    public NoteService(INoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }

    public async Task<Result<List<StudentNotesResponseDto>, string>> GetStudentNotesByParentIdAsync(Guid parentId)
    {
        var result = new Result<List<StudentNotesResponseDto>, string>();
            
        try
        {
            var notes = await _noteRepository.GetStudentNotesByParentIdAsync(parentId);
                
            return notes.Count == 0 ? result.Failure("No notes found for your children", (int)HttpStatusCode.NotFound) : result.Success(notes);
        }
        catch (Exception ex)
        {
            return result.Failure($"Error retrieving student notes: {ex.Message}", 500);
        }
    }
    public async Task<Result<StudentNotesResponseDto, string>> GetStudentNotesByStudentIdForParentAsync(Guid parentId, Guid studentId)
    {
        var result = new Result<StudentNotesResponseDto, string>();

        try
        {
            // Check if the student belongs to this parent
            var studentExists = await _noteRepository.CheckStudentBelongsToParentAsync(parentId, studentId);
        
            if (!studentExists)
            {
                return result.Failure("Student not found or does not belong to this parent", (int)HttpStatusCode.NotFound);
            }

            // Get the student notes
            var notes = await _noteRepository.GetStudentNotesByStudentId(studentId);
        
            if (notes.Count == 0)
            {
                return result.Failure("No notes found for this student", (int)HttpStatusCode.NotFound);
            }
        
            // Get student details
            var studentDetails = await _noteRepository.GetStudentDetailsAsync(studentId);
        
            var response = new StudentNotesResponseDto
            {
                StudentName = studentDetails.StudentName,
                SchoolLevel = studentDetails.SchoolLevel,
                GroupName = studentDetails.GroupName,
                Notes = notes
            };

            return result.Success(response);
        }
        catch (Exception ex)
        {
            return result.Failure($"Error retrieving student notes: {ex.Message}", 500);
        }
    }
}