// TeacherController.cs
using Dirassati_Backend.Dtos;
using Dirassati_Backend.Features.Teachers.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.Teachers.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/teacher")]
    public class TeacherController : ControllerBase
    {
        private readonly TeacherServices _teacherServices;

        public TeacherController(TeacherServices teacherServices)
        {
            _teacherServices = teacherServices;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTeacher([FromBody] TeacherInfosDTO teacherDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var teacherId = await _teacherServices.RegisterTeacherAsync(teacherDto);
                return CreatedAtAction(nameof(GetTeacher), new { id = teacherId }, null);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        


        [HttpGet("{id}")]
        public IActionResult GetTeacher(Guid id)
        {
            return Ok(new { TeacherId = id });
        }
    }
}