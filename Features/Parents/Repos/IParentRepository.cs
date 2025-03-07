using Dirassati_Backend.Domain.Models;
using Dirassati_Backend.Features.Parents.Dtos;
using static Dirassati_Backend.Features.Parents.Dtos.ParentDtos;

namespace Dirassati_Backend.Features.Parents.Repositories
{
    public interface IParentRepository
    {
        Task<IEnumerable<GetParentDto>> GetAllAsync();
        Task<GetParentDto?> GetParentByIdAsync(Guid parentId);
        Task<Parent> CreateAsync(Parent parent);//Note : this is just for testing purposes
        Task<GetParentDto?> UpdateAsync(UpdateParentDto updateDto);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<getStudentDto>> GetStudentsByParentIdAsync(Guid parentId);
        Task<getStudentParentDto?> GetParentByStudentIdAsync(Guid studentId);
    }
}
