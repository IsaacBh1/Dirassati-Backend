using Dirassati_Backend.Data.Models;

namespace Dirassati_Backend.Features.Groups.Repos
{
    public interface IGroupRepository
    {
        Task<Group> GetGroupWithStudentsAsync(int groupId);
    }
}