
using System.Net;
using Dirassati_Backend.Features.Auth.Register.Dtos;
using Dirassati_Backend.Features.Auth.Register.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.Auth.SignUp
{


    [Tags("Employee Authentication")]
    [Route("api/auth")]
    [ApiController]
    [Produces("application/json")]
    public class SignUpController(RegisterService service, VerifyEmailService verifyEmailService, SendCridentialsService sendCridentialsService) : BaseController
    {
        private readonly RegisterService _registerService = service;
        private readonly VerifyEmailService _verifyEmailService = verifyEmailService;
        private readonly SendCridentialsService _sendCridentialsService = sendCridentialsService;
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(EmployeeDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Resgister(RegisterDto dto)
        {
            var result = await _registerService.Register(dto);
            if (result.IsSuccess)
                return Ok(result.Value);
            return BadRequest(result.Errors);
        }


        [AllowAnonymous]
        [HttpGet("register/verify-email", Name = "VerifyEmail")]
        public async Task<ActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
        {

            var result = await _verifyEmailService.VerifyEmailAsync(email, token);
            if (!result.IsSuccess)
                return HandleResult(result);
            var credResult = await _sendCridentialsService.SendCridentialsAsync(email);
            return HandleResult(credResult);

        }



    }
}
