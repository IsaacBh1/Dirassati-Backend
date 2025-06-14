using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Dirassati_Backend.Features.RelationshipToStudent.DTOs;
using Dirassati_Backend.Persistence;
using Microsoft.AspNetCore.Authorization;

namespace Dirassati_Backend.Features.ParentRelationShip
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelationshipToStudentController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Employee,Teacher,Parent")]

        public async Task<ActionResult<IEnumerable<GetRelationshipsDto>>> GetAll()
        {
            var relationships = await context.ParentRelationshipToStudentTypes.Select(r => new GetRelationshipsDto { Id = r.Id, Name = r.Name }).ToListAsync();
            return Ok(relationships);
        }
    }
}
