using Dirassati_Backend.Common;
using Dirassati_Backend.Features.Groups.Dtos;

namespace Dirassati_Backend.Features.Groups.Services
{
    public interface IGroupServices
    {
        Task<Result<GroupDto, string>> AddGroupAsync(AddGroupDto addGroupDto, string schoolId);

        /// <summary>
        /// Retrieves groups for a school, optionally filtered by level.
        /// </summary>
        /// <param name="levelId">The optional level ID to filter groups by. If null, returns all groups for the school.</param>
        /// <param name="schoolId">The ID of the school to retrieve groups for.</param>
        /// <returns>A result containing a list of groups or an error message.</returns>
        /// <remarks>
        /// This method has two behaviors:
        /// 1. When levelId is null: Returns all groups belonging to the specified school.
        /// 2. When levelId is provided: Returns only groups associated with the specified level in the school.
        /// </remarks>
        Task<Result<List<GroupListingDto>, string>> GetAllGroupsOrByLevelIdAsync(int? levelId, string schoolId);
    }
}