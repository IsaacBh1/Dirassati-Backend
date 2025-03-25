using Dirassati_Backend.Features.Absences.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.Abcenses.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AbsenceController : ControllerBase
    {
        private readonly AbsenceService _absenceService;

        public AbsenceController(AbsenceService absenceService)
        {
            _absenceService = absenceService;
        }

        [HttpPost("mark/{groupId}")]
        public async Task<IActionResult> MarkAbsences(int groupId, [FromBody] List<Guid> absentStudentIds)
        {
            await _absenceService.MarkAbsencesAsync(groupId, absentStudentIds);
            return Ok();
        }
        [HttpPost("test-broadcast")]
        public async Task<IActionResult> TestBroadcast()
        {
            await _absenceService.TestBroadcastNotification();
            return Ok(new { Message = "Broadcast test initiated" });
        }
    }
}