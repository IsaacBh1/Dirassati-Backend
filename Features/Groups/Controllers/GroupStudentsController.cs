using System.Security.Claims;
using Dirassati_Backend.Common;
using Dirassati_Backend.Features.Groups.Dtos;
using Dirassati_Backend.Features.Groups.Services;
using Dirassati_Backend.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Dirassati_Backend.Features.Groups.Controllers;

[Tags("Group")]
[Route("api/group/students")]
[ApiController]
public class 
    GroupStudentsController(AppDbContext context, IGroupServices groupServices) : BaseController
{
    private readonly AppDbContext _context = context;
    private readonly IGroupServices _groupServices = groupServices;


    [HttpGet("by-group/{groupId}")]
    public async Task<IActionResult> GetStudentsByGroup(Guid groupId)
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
   
    /// <summary>
    /// Creates a new group with optional student assignments
    /// </summary>
    /// <param name="addGroupDto">Group information and optional student IDs</param>
    /// <returns>The created group with details</returns>
    [HttpPost]
    public async Task<IActionResult> AddGroup(AddGroupDto addGroupDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
            
        var schoolId = User.FindFirstValue("SchoolId");
        if (string.IsNullOrEmpty(schoolId))
            return Unauthorized("School ID is missing from user claims");
                
        var result = await _groupServices.AddGroupAsync(addGroupDto, schoolId);
        return HandleResult(result);
    }

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
                
        var result = await _groupServices.GetGroupsByLevelIdAsync(levelId, schoolId);
        return HandleResult(result);
    }

}
