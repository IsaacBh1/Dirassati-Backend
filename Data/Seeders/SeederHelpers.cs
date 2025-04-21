using Dirassati_Backend.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Data.Seeders
{
    public class SeederHelpers(
        AppDbContext dbContext
    )
    {
        private readonly AppDbContext _dbContext = dbContext;

        public async Task<string?> GetSchoolById(string schoolId)
        {
            var result = await _dbContext.Schools.FirstOrDefaultAsync(s => s.SchoolId == Guid.Parse(schoolId));
            return result?.SchoolId.ToString();
        }
    }
}