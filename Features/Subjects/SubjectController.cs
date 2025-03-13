using Dirassati_Backend.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Dirassati_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SubjectsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SubjectsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Subjects/BySchoolLevel
        [HttpGet("BySchoolLevel")]
        public async Task<IActionResult> GetSubjectsBySchoolLevel()
        {
            // Récupérer le SchoolLevelId depuis le token JWT
            var schoolLevelClaim = User.Claims.FirstOrDefault(c => c.Type == "SchoolLevelId");

            if (schoolLevelClaim == null || !int.TryParse(schoolLevelClaim.Value, out int schoolLevelId))
            {
                return BadRequest("SchoolLevelId non trouvé dans le token ou format invalide");
            }

            // Vérifier que le SchoolLevelId est valide
            if (!Enum.IsDefined(typeof(SchoolTypeEnum), schoolLevelId))
            {
                return BadRequest("SchoolLevelId invalide");
            }

            // Convertir en enum
            var schoolLevel = (SchoolTypeEnum)schoolLevelId;

            // Récupérer les matières correspondantes
            var subjects = await _context.Subjects
                .Where(s => s.SchoolType == schoolLevel)
                .ToListAsync();

            return Ok(new
            {
                SchoolLevelId = schoolLevelId,
                Subjects = subjects
            });
        }
    }
}