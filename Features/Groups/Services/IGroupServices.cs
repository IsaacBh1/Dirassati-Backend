using Dirassati_Backend.Common;
using Dirassati_Backend.Features.Groups.Dtos;

namespace Dirassati_Backend.Features.Groups.Services
{
    public interface IGroupServices
    {
        Task<Result<GroupDto, string>> AddGroupAsync(AddGroupDto addGroupDto, string schoolId);
        Task<Result<List<GroupListingDto>, string>> GetGroupsByLevelIdAsync(int levelId, string schoolId);
    }
}