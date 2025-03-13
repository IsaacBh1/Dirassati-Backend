using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Dirassati_Backend.Features.ParentRelationShip.DTOs;
using Persistence;

namespace Dirassati_Backend.Features.ParentRelationShip
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelationshipToStudentController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetRelationshipsDTO>>> GetAll()
        {
            var relationships = await context.ParentRelationshipToStudentTypes.Select(r => new GetRelationshipsDTO{Id = r.Id, Name = r.Name}).ToListAsync();
            return Ok(relationships);
        }
    }
}
