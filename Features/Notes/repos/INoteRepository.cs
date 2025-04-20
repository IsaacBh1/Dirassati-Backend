using Dirassati_Backend.Data.Models;
using System.Threading.Tasks;
using Dirassati_Backend.Features.Notes.Dtos;

namespace Dirassati_Backend.Features.Notes.Repos
{
    public interface INoteRepository
    {
        Task<Note> AddNoteAsync(Note note);
        Task BulkAddAsync(IEnumerable<Note> notes);
        Task<List<StudentNotesResponseDto>> GetStudentNotesByParentIdAsync(Guid parentId);
        public  Task<List<StudentNoteDto>> GetStudentNotesByStudentId(Guid Id);
        Task<bool> CheckStudentBelongsToParentAsync(Guid parentId, Guid studentId);
        public  Task<SimpleStudentDetailsDto> GetStudentDetailsAsync(Guid studentId);


    }
}