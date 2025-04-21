using System.Security.Claims;
using Dirassati_Backend.Common;
using Dirassati_Backend.Features.Groups.Services;
using Dirassati_Backend.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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
        public async Task<IActionResult> GetGroupsByLevel(int levelId)
        {
            var schoolId = User.FindFirstValue("SchoolId");
            if (string.IsNullOrEmpty(schoolId))
                return Unauthorized("School ID is missing from user claims");

            var result = await _groupServices.GetAllGroupsOrByLevelIdAsync(levelId, schoolId);
            return HandleResult(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllGroups()
        {
            var schoolId = User.FindFirstValue("SchoolId");
            if (string.IsNullOrEmpty(schoolId))
                return Unauthorized("School ID is missing from user claims");

            var result = await _groupServices.GetAllGroupsOrByLevelIdAsync(null, schoolId);
            return HandleResult(result);
        }
    }
}
