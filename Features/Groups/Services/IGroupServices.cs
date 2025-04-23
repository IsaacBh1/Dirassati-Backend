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

        /// <summary>
        /// Updates an existing group's information and optionally assigns students to it
        /// </summary>
        /// <param name="groupId">The ID of the group to update</param>
        /// <param name="updateGroupDto">The updated group information</param>
        /// <param name="schoolId">The ID of the school that owns the group</param>
        /// <returns>The updated group information</returns>
        Task<Result<GroupDto, string>> UpdateGroupAsync(Guid groupId, UpdateGroupDto updateGroupDto, string schoolId);

        /// <summary>
        /// Deletes a group from the system
        /// </summary>
        /// <param name="groupId">The ID of the group to delete</param>
        /// <param name="schoolId">The ID of the school that owns the group</param>
        /// <returns>A success or error message</returns>
        Task<Result<Unit, string>> DeleteGroupAsync(Guid groupId, string schoolId);

        /// <summary>
        /// Assigns a student to a specific group
        /// </summary>
        /// <param name="assignDto">The assignment information containing student and group IDs</param>
        /// <param name="schoolId">The ID of the school</param>
        /// <returns>The updated student information</returns>
        Task<Result<GroupDto, string>> AssignStudentToGroupAsync(AssignStudentToGroupDto assignDto, string schoolId);
    }
}