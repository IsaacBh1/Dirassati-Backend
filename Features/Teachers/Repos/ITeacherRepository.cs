using Dirassati_Backend.Data.Models;

namespace Dirassati_Backend.Repositories
{
    public interface ITeacherRepository
    {
        Task AddTeacherAsync(Teacher teacher);
        Task SaveChangesAsync();
    }
}
