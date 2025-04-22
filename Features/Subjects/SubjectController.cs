using Dirassati_Backend.Data.Enums;
using Dirassati_Backend.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Features.Subjects.Dtos;

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

        ///BySchoolLevel
        [HttpGet("BySchoolLevel")]
        public async Task<IActionResult> GetSubjectsBySchoolLevel()
        {


            var schoolLevelClaim = User.Claims.FirstOrDefault(c => c.Type == "SchoolTypeId");

            if (schoolLevelClaim == null || !int.TryParse(schoolLevelClaim.Value, out int SchoolTypeId))

            {
                return BadRequest("SchoolTypeId not found");
            }


            if (!Enum.IsDefined(typeof(SchoolTypeEnum), SchoolTypeId))
            {
                return BadRequest("SchoolTypeId invalide");
            }


            var schoolLevel = (SchoolTypeEnum)SchoolTypeId;


            var subjects = await _context.Subjects
                .Where(s => s.SchoolType == schoolLevel)

                .Select(s => new GetSubjectsBySchoolLevelDto
                {
                    SubjectId = s.SubjectId,
                    Name = s.Name
                })
                .ToListAsync();

            return Ok(new
            {
                SchoolTypeId = SchoolTypeId,
                Subjects = subjects
            });
        }
    }
}