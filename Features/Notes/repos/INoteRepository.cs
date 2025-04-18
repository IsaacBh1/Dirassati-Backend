using Dirassati_Backend.Data.Models;
using System.Threading.Tasks;

namespace Dirassati_Backend.Features.Notes.Repos
{
    public interface INoteRepository
    {
        Task<Note> AddNoteAsync(Note note);
        Task BulkAddAsync(IEnumerable<Note> notes);
    }
}