using Dirassati_Backend.Features.Groups.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
namespace Dirassati_Backend.Features.Groups.Controllers
{
    [Tags("Group")]
    [Route("api/group/students")]
    [ApiController]
    public class GroupStudentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GroupStudentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("by-group/{groupId}")]
        public async Task<IActionResult> GetStudentsByGroup(int groupId)
        {
            var groupExists = await _context.Groups.AnyAsync(g => g.GroupId == groupId);
            if (!groupExists)
            {
                return NotFound("Group not found");
            }

            var students = await _context.Students
                .Where(s => s.GroupId == groupId)
                .Include(s => s.Parent)
                    .ThenInclude(p => p.User)
                .Select(s => new StudentGroupDto
                {
                    StudentId = s.StudentId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    StudentIdNumber = s.StudentIdNumber ?? string.Empty,
                    ParentName = $"{s.Parent.User.FirstName} {s.Parent.User.LastName}",
                    ParentContact = s.Parent.User.PhoneNumber ?? string.Empty,
                    ParentEmail = s.Parent.User.Email ?? string.Empty
                })
                .ToListAsync();

            return Ok(students);
        }
    }
}
