using Dirassati_Backend.Common;
using Dirassati_Backend.Features.Notes.Dtos;

namespace Dirassati_Backend.Features.Notes.Services;

public interface INotesServices
{
    public Task<Result<List<StudentNotesResponseDto>, string>> GetStudentNotesByParentIdAsync(Guid parentId);
    public Task<Result<StudentNotesResponseDto, string>> GetStudentNotesByStudentIdForParentAsync(Guid parentId, Guid studentId);

}