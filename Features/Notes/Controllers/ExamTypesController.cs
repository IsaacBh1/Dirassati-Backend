using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Dirassati_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamTypesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExamTypesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExamType>>> GetExamTypes()
        {
            var examTypes = await _context.ExamTypes.ToListAsync();
            return Ok(examTypes);
        }
    }
}
