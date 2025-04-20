using System.Security.Claims;
using Dirassati_Backend.Common;
using Dirassati_Backend.Features.Classrooms;
using Dirassati_Backend.Features.Classrooms;
using Dirassati_Backend.Features.Classrooms.Dtos;
using Dirassati_Backend.Features.Classrooms.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.Classrooms
{
    [Tags("Classrooms")]
    [Route("api/classrooms")]
    [ApiController]
    public class ClassroomsController(IClassroomServices classroomServices) : BaseController
    {
        /// <summary>
        /// Creates a new classroom
        /// </summary>
        /// <param name="addClassroomDto">Classroom information to add</param>
        /// <returns>The newly created classroom</returns>
        [HttpPost]
        public async Task<ActionResult<ClassroomDto>> AddClassroom(AddClassroomDto addClassroomDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var schoolId = User.FindFirstValue("SchoolId");
            if (string.IsNullOrEmpty(schoolId))
                return Unauthorized("School ID is missing from user claims");

            var result = await classroomServices.AddClassroomAsync(addClassroomDto, schoolId);
            return HandleResult(result);
        }

        /// <summary>
        /// Gets all classrooms for a specific school level
        /// </summary>
        /// <param name="levelId">The school level ID</param>
        /// <returns>List of classrooms for the specified level</returns>
        [HttpGet("by-level/{levelId:int}")]
        public async Task<ActionResult<ClassroomDto>> GetClassroomsByLevel(int levelId)
        {
            var schoolId = User.FindFirstValue("SchoolId");
            if (string.IsNullOrEmpty(schoolId))
                return Unauthorized("School ID is missing from user claims");

            var result = await classroomServices.GetClassroomsBySchoolLevelAsync(levelId, schoolId);
            return HandleResult(result);
        }

        /// <summary>
        /// Gets all classrooms for the current school
        /// </summary>
        /// <returns>List of all classrooms in the school</returns>
        [HttpGet]
        public async Task<ActionResult<List<ClassroomDto>>> GetAllClassrooms()
        {
            var schoolId = User.FindFirstValue("SchoolId");
            if (string.IsNullOrEmpty(schoolId))
                return Unauthorized("School ID is missing from user claims");

            var result = await classroomServices.GetAllClassroomsBySchoolAsync(schoolId);
            return HandleResult(result);
        }

        /// <summary>
        /// Gets detailed information about a specific classroom, including associated groups
        /// </summary>
        /// <param name="classroomId">The classroom ID</param>
        /// <returns>Detailed information about the classroom</returns>
        [HttpGet("{classroomId}")]
        public async Task<ActionResult<ClassroomDetailDto>> GetClassroomDetails(Guid classroomId)
        {
            var schoolId = User.FindFirstValue("SchoolId");
            if (string.IsNullOrEmpty(schoolId))
                return Unauthorized("School ID is missing from user claims");

            var result = await classroomServices.GetClassroomDetailsAsync(classroomId, schoolId);
            return HandleResult(result);
        }

        /// <summary>
        /// Updates an existing classroom
        /// </summary>
        /// <param name="classroomId">The classroom ID</param>
        /// <param name="updateClassroomDto">Updated classroom information</param>
        /// <returns>The updated classroom</returns>
        [HttpPut("{classroomId}")]
        public async Task<ActionResult<ClassroomDto>> UpdateClassroom(Guid classroomId, UpdateClassroomDto updateClassroomDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var schoolId = User.FindFirstValue("SchoolId");
            if (string.IsNullOrEmpty(schoolId))
                return Unauthorized("School ID is missing from user claims");

            var result = await classroomServices.UpdateClassroomAsync(classroomId, updateClassroomDto, schoolId);
            return HandleResult(result);
        }
    }
}