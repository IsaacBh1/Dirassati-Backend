using Dirassati_Backend.Features.Common;
using Dirassati_Backend.Features.Students.DTOs;
namespace Dirassati_Backend.Features.Students.Repositories
{

    public interface IStudentRepository
    {
        Task<PaginatedResult<StudentDto>> GetStudentsBySchoolIdAsync(Guid schoolId, int page, int pageSize);
        Task<bool> SchoolExistsAsync(Guid schoolId);
        Task<StudentDetailsDto?> GetStudentByIdAsync(Guid studentId);
    }
}
