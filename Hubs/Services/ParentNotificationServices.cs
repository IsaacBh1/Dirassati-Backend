
using Dirassati_Backend.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Hubs.Services;

public class ParentNotificationServices(AppDbContext dbContext) : IParentNotificationServices
{
    private readonly AppDbContext _dbContext = dbContext;

    public async Task<List<Guid>> GetSchoolIdsByParentIdAsync(Guid parentId)
    {
        return await _dbContext.Students
            .Where(s => s.ParentId == parentId)
            .Select(s => s.SchoolId)
            .Distinct()
            .ToListAsync();
    }
}
