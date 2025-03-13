using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Domain.Models;
using Dirassati_Backend.Features.Common;
using Dirassati_Backend.Features.Parents.Dtos;

namespace Dirassati_Backend.Features.Parents.Repositories
{
    public interface IParentRepository
    {
        Task<IEnumerable<GetParentDto>> GetAllBySchoolIdAsync(Guid schoolId);
        Task<PaginatedResult<GetParentDto>> GetAllBySchoolIdAsync(Guid schoolId, int pageNumber, int pageSize);
        Task<GetParentDto?> GetParentByIdAsync(Guid parentId);
        Task<Parent> CreateAsync(Parent parent);//Note : this is just for testing purpos es
        Task<GetParentDto?> UpdateAsync(UpdateParentDto updateDto);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<getStudentDto>> GetStudentsByParentIdAsync(Guid parentId);
        Task<getStudentParentDto?> GetParentByStudentIdAsync(Guid studentId);
    }
}
