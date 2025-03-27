using Dirassati_Backend.Data.Models;
using Microsoft.EntityFrameworkCore;
using Persistence;
namespace Dirassati_Backend.Features.Groups.Repos
{


    public class GroupRepository : IGroupRepository
    {
        private readonly AppDbContext _context;

        public GroupRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Group> GetGroupWithStudentsAsync(int groupId)
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
    }
}