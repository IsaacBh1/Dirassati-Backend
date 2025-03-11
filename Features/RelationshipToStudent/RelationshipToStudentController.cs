using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Dirassati_Backend.Domain.Models;
using Persistence;

namespace Dirassati_Backend.Features.ParentRelationShip
{
    [Route("[controller]")]
    [ApiController]
    public class RelationshipToStudentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RelationshipToStudentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RelationshipToStudent>>> GetAll()
        {
            var relationships = await _context.RelationshipToStudents.ToListAsync();
            return Ok(relationships);
        }
    }
}
