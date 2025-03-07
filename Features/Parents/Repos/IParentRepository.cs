using Dirassati_Backend.Domain.Models;
using Dirassati_Backend.Features.Parents.Dtos;

namespace Dirassati_Backend.Features.Parents.Repositories
{
    public interface IParentRepository
    {
        Task<IEnumerable<Parent>> GetAllAsync();
        Task<Parent?> GetByIdAsync(Guid id);
        Task<Parent> CreateAsync(Parent parent);
        Task<Parent?> UpdateAsync(Parent parent);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<getStudentDto>> GetStudentsByParentIdAsync(Guid parentId);
    }
}
