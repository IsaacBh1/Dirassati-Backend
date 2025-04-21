using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Persistence;

namespace Dirassati_Backend.Repositories
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly AppDbContext _context;
        public TeacherRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddTeacherAsync(Teacher teacher)
        {
            await _context.Teachers.AddAsync(teacher);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
