
using System.Net;
using Dirassati_Backend.Domain.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.Auth.SignUp
{


    [Route("api/auth")]
    [ApiController]
    [Produces("application/json")]
    public class SignUpController : ControllerBase
    {
        private readonly RegisterService _service;

        public SignUpController(RegisterService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(Employee), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Resgister(RegisterDto dto)
        {
            var employee = await _service.Register(dto);
            if (employee is null)
                return BadRequest();
            return Ok(employee);
        }
    }
}
