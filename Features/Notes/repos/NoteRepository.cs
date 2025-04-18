using Dirassati_Backend.Data.Models;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Threading.Tasks;

namespace Dirassati_Backend.Features.Notes.Repos
{
    public class NoteRepository : INoteRepository
    {
        private readonly AppDbContext _context;

        public NoteRepository(AppDbContext context)
        {
            _context = context;
        }

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
    }
}