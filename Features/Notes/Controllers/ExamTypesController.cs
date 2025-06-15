using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Features.Notes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamTypesController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpGet]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult<IEnumerable<ExamType>>> GetExamTypes()
        {
            var examTypes = await _context.ExamTypes.ToListAsync();
            return Ok(examTypes);
        }
    }
}