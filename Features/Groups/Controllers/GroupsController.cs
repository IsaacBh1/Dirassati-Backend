using System.Security.Claims;
using Dirassati_Backend.Common;
using Dirassati_Backend.Features.Groups.Dtos;
using Dirassati_Backend.Features.Groups.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.Groups.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController(IGroupServices groupServices) : BaseController
    {
        private readonly IGroupServices _groupServices = groupServices;

        /// <summary>
        /// Gets all groups for a specific school level
        /// </summary>
        /// <param name="levelId">The school level ID</param>
        /// <returns>List of groups for the specified level</returns>
        [HttpGet("by-level/{levelId:int}")]
        [Authorize(Roles = "Employee,Teacher")]

        public async Task<IActionResult> GetGroupsByLevel(int levelId)
        {
            var schoolId = User.FindFirstValue("SchoolId");
            if (string.IsNullOrEmpty(schoolId))
                return Unauthorized("School ID is missing from user claims");

            var result = await _groupServices.GetAllGroupsOrByLevelIdAsync(levelId, schoolId);
            return HandleResult(result);
        }

        /// <summary>
        /// Gets all groups for the current school
        /// </summary>
        /// <returns>List of all groups in the school</returns>
        [HttpGet]
        [Authorize(Roles = "Employee,Teacher")]

        public async Task<IActionResult> GetAllGroups()
        {
            var schoolId = User.FindFirstValue("SchoolId");
            if (string.IsNullOrEmpty(schoolId))
                return Unauthorized("School ID is missing from user claims");

            var result = await _groupServices.GetAllGroupsOrByLevelIdAsync(null, schoolId);
            return HandleResult(result);
        }

        /// <summary>
        /// Updates an existing group's information
        /// </summary>
        /// <param name="groupId">ID of the group to update</param>
        /// <param name="updateGroupDto">Group information to update</param>
        /// <returns>The updated group information</returns>
        [HttpPut("{groupId}")]
        [Authorize(Roles = "Employee")]

        public async Task<IActionResult> UpdateGroup(Guid groupId, [FromBody] UpdateGroupDto updateGroupDto)
        {
            var schoolId = User.FindFirstValue("SchoolId");
            if (string.IsNullOrEmpty(schoolId))
                return Unauthorized("School ID is missing from user claims");

            var result = await _groupServices.UpdateGroupAsync(groupId, updateGroupDto, schoolId);
            return HandleResult(result);
        }

        /// <summary>
        /// Deletes a group
        /// </summary>
        /// <param name="groupId">ID of the group to delete</param>
        /// <returns>Success or error message</returns>
        [HttpDelete("{groupId}")]
        [Authorize(Roles = "Employee")]

        public async Task<IActionResult> DeleteGroup(Guid groupId)
        {
            var schoolId = User.FindFirstValue("SchoolId");
            if (string.IsNullOrEmpty(schoolId))
                return Unauthorized("School ID is missing from user claims");

            var result = await _groupServices.DeleteGroupAsync(groupId, schoolId);
            return HandleResult(result);
        }

        /// <summary>
        /// Assigns a student to a group
        /// </summary>
        /// <param name="assignStudentDto">Assignment information containing student and group IDs</param>
        /// <returns>The updated group information</returns>
        [HttpPost("assign-student")]
        [Authorize(Roles = "Employee")]

        public async Task<IActionResult> AssignStudentToGroup([FromBody] AssignStudentToGroupDto assignStudentDto)
        {
            var schoolId = User.FindFirstValue("SchoolId");
            if (string.IsNullOrEmpty(schoolId))
                return Unauthorized("School ID is missing from user claims");

            var result = await _groupServices.AssignStudentToGroupAsync(assignStudentDto, schoolId);
            return HandleResult(result);
        }
    }
}
