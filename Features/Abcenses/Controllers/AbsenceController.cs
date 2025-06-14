using System.Security.Claims;
using Dirassati_Backend.Features.Abcenses.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.Abcenses.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AbsenceController(AbsenceService absenceService) : ControllerBase
    {
        private readonly AbsenceService _absenceService = absenceService;

        /// <summary>
        /// Mark students as absent for a group
        /// </summary>
        /// <param name="groupId">ID of the group</param>
        /// <param name="absentStudentIds">List of student IDs to mark as absent</param>
        /// <returns>Success status</returns>
        [HttpPost("mark/{groupId}")]
        [Authorize(Roles = "Teacher")]

        public async Task<IActionResult> MarkAbsences(Guid groupId, [FromBody] List<Guid> absentStudentIds)
        {
            await _absenceService.MarkAbsencesAsync(groupId, absentStudentIds);
            return Ok();
        }

        /// <summary>
        /// Test the broadcast notification system
        /// </summary>
        /// <returns>Success status</returns>
        [HttpPost("test-broadcast")]
        public async Task<IActionResult> TestBroadcast()
        {
            await _absenceService.TestBroadcastNotification();
            return Ok(new { Message = "Broadcast test initiated" });
        }

        /// <summary>
        /// Get all absences for a specific student, verifying the parent relationship
        /// </summary>
        /// <param name="studentId">ID of the student</param>
        /// <param name="parentId">ID of the parent making the request</param>
        /// <returns>List of absences for the student</returns>
        [HttpGet("student/{studentId}/absences")]
        [Authorize(Roles = "Employee,Teacher, Parent")]

        public async Task<IActionResult> GetStudentAbsences(Guid studentId, [FromQuery] Guid parentId)
        {
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var parentIdClaim = User.Claims.First(c => c.Equals(parentId)).Value;

            if (userRole == null || (userRole != "Employee" && !(userRole == "Parent" && parentIdClaim == parentId.ToString())))
            {
                return Forbid("You are not authorized to access this resource.");
            }
            try
            {
                var absences = await _absenceService.GetStudentAbsencesAsync(studentId, parentId);
                return Ok(absences);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}