using Dirassati_Backend.Data.Enums;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Features.Subjects.Dtos;

namespace Dirassati_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SubjectsController(AppDbContext context)
        {
            _context = context;
        }

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

        [HttpGet]
        public async Task<IActionResult> GetAllSubjects()
        {
            var subjects = await _context.Subjects.ToListAsync();
            return Ok(subjects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubject(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }
            return Ok(subject);
        }
        [HttpPost]
        public async Task<IActionResult> CreateSubject([FromBody] Subject subject)
        {
            if (!Enum.IsDefined(typeof(SchoolTypeEnum), subject.SchoolType))
            {
                return BadRequest("Invalid SchoolType");
            }

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSubject), new { id = subject.SubjectId }, subject);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] Subject updatedSubject)
        {
            if (id != updatedSubject.SubjectId)
            {
                return BadRequest("Subject ID mismatch.");
            }

            if (!Enum.IsDefined(typeof(SchoolTypeEnum), updatedSubject.SchoolType))
            {
                return BadRequest("Invalid SchoolType");
            }

            _context.Entry(updatedSubject).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SubjectExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }

            _context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        private bool SubjectExists(int id)
        {
            return _context.Subjects.Any(e => e.SubjectId == id);
        }

    }
}