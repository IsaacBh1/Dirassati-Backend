using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Dirassati_Backend.Features.Groups.Repos
{
    public class GroupRepository(AppDbContext context) : IGroupRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<Group> GetGroupWithStudentsAsync(Guid groupId)
        {
            var group = await _context.Groups
                .Include(g => g.Students)
                .ThenInclude(s => s.Parent)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null)
            {
                throw new KeyNotFoundException($"Group with ID {groupId} not found.");
            }

            return group;
        }

        public async Task<Student?> GetStudentWithParentAsync(Guid studentId)
        {
            return await _context.Students
                .Include(s => s.Parent)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }
    }
}