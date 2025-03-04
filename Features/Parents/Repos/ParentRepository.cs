using Dirassati_Backend.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Dirassati_Backend.Features.Parents.Repositories
{
    public class ParentRepository : IParentRepository
    {
        private readonly AppDbContext _context;

        public ParentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Parent>> GetAllAsync()
        {
            return await _context.Parents.Include(p => p.User).Include(p => p.relationshipToStudent).ToListAsync();
        }

        public async Task<Parent?> GetByIdAsync(Guid id)
        {
            return await _context.Parents.Include(p => p.User).Include(p => p.relationshipToStudent)
                                         .FirstOrDefaultAsync(p => p.ParentId == id);
        }

        public async Task<Parent> CreateAsync(Parent parent)
        {
            _context.Parents.Add(parent);
            await _context.SaveChangesAsync();
            return parent;
        }

        public async Task<Parent?> UpdateAsync(Parent parent)
        {
            var existingParent = await _context.Parents.FindAsync(parent.ParentId);
            if (existingParent == null)
                return null;

            existingParent.UserId = parent.UserId;
            existingParent.Occupation = parent.Occupation;
            existingParent.RelationshipToStudentId = parent.RelationshipToStudentId;

            await _context.SaveChangesAsync();
            return existingParent;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var parent = await _context.Parents.FindAsync(id);
            if (parent == null)
                return false;

            _context.Parents.Remove(parent);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
