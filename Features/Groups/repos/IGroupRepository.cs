using Dirassati_Backend.Data.Models;

namespace Dirassati_Backend.Features.Groups.Repos
{
    public interface IGroupRepository
    {
        Task<Group> GetGroupWithStudentsAsync(Guid groupId);
        Task<Student?> GetStudentWithParentAsync(Guid studentId);
    }
}