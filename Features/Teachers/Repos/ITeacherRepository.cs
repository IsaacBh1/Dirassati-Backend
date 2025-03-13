using Dirassati_Backend.Domain.Models;
using System.Threading.Tasks;

namespace Dirassati_Backend.Repositories
{
    public interface ITeacherRepository
    {
        Task AddTeacherAsync(Teacher teacher);
        Task SaveChangesAsync();
    }
}
