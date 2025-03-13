using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Parents.Dtos;
using Dirassati_Backend.Features.Parents.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.Parents.Controllers
{
    [Route("api/parents")]
    [ApiController]
    public class ParentController : ControllerBase
    {
        private readonly IParentRepository _parentRepository;

        public ParentController(IParentRepository parentRepository)
        {
            _parentRepository = parentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(Guid SchoolId)
        {
            var parents = await _parentRepository.GetAllBySchoolIdAsync(SchoolId);
            return Ok(parents);
        }

        [HttpGet("paginated")]
        public async Task<IActionResult> GetAllPaginated(Guid SchoolId, int pageNumber, int pageSize)
        {
            var paginatedParents = await _parentRepository.GetAllBySchoolIdAsync(SchoolId, pageNumber, pageSize);
            return Ok(paginatedParents);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var parent = await _parentRepository.GetParentByIdAsync(id);
            if (parent == null) return NotFound();
            return Ok(parent);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Parent parent)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdParent = await _parentRepository.CreateAsync(parent);
            return CreatedAtAction(nameof(GetById), new { id = createdParent.ParentId }, createdParent);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateParentDto parent)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != parent.ParentId)
                return BadRequest("Mismatched Parent ID");

            var updatedParent = await _parentRepository.UpdateAsync(parent);
            if (updatedParent == null) return NotFound();
            return Ok(updatedParent);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _parentRepository.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpGet("{parentId:guid}/students")]
        public async Task<IActionResult> GetStudentsByParent(Guid parentId)
        {
            var students = await _parentRepository.GetStudentsByParentIdAsync(parentId);

            if (students == null || !students.Any())
                return NotFound("No students found for the provided parent ID.");

            return Ok(students);
        }

        [HttpGet("{studentId:guid}/parent")]
        public async Task<IActionResult> GetParentByStudentId(Guid studentId)
        {
            var parent = await _parentRepository.GetParentByStudentIdAsync(studentId);

            if (parent == null)
                return NotFound("No parent found for the provided student ID.");

            return Ok(parent);
        }
    }
}
