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
            // Load the existing parent including the associated User.
            var existingParent = await _context.Parents
                                                 .Include(p => p.User)
                                                 .FirstOrDefaultAsync(p => p.ParentId == parent.ParentId);

            if (existingParent == null)
                return null;

            // Update Parent's own properties.
            existingParent.Occupation = parent.Occupation;
            existingParent.RelationshipToStudentId = parent.RelationshipToStudentId;


            if (parent.User != null && existingParent.User != null)
            {
                existingParent.User.FirstName = parent.User.FirstName;
                existingParent.User.LastName = parent.User.LastName;
                existingParent.User.Email = parent.User.Email;
                existingParent.User.PhoneNumber = parent.User.PhoneNumber;
            }

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

        public async Task<IEnumerable<Student>> GetStudentsByParentIdAsync(Guid parentId)
        {
            return await _context.Students.Where(s => s.ParentId == parentId).ToListAsync();
        }
    }
}
