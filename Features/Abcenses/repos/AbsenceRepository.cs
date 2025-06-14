using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Features.Absences.Repos
{
    public class AbsenceRepository(AppDbContext context) : IAbsenceRepository
    {
        private readonly AppDbContext _context = context;

        public async Task AddAbsenceAsync(Absence absence)
        {
            await _context.Absences.AddAsync(absence);
        }

        public async Task UpdateAbsenceAsync(Absence absence)
        {
            _context.Absences.Update(absence);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<Absence>> GetAbsencesByStudentIdsAsync(List<Guid> studentIds, bool isNotified)
        {
            return await _context.Absences
                .Where(a => studentIds.Contains(a.StudentId))
                .Where(a => a.IsNotified == isNotified)
                .ToListAsync();
        }

        public async Task<List<Absence>> GetAbsencesByStudentIdAsync(Guid studentId)
        {
            return await _context.Absences
                .Include(a => a.Student)
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();
        }
    }
}