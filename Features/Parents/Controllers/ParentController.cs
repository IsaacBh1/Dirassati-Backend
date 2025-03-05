using Dirassati_Backend.Domain.Models;
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
        public async Task<IActionResult> GetAll()
        {
            var parents = await _parentRepository.GetAllAsync();
            return Ok(parents);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var parent = await _parentRepository.GetByIdAsync(id);
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
        public async Task<IActionResult> Update(Guid id, [FromBody] Parent parent)
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
    }
}
