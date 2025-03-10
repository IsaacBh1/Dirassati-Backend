
using System.Net;
using Dirassati_Backend.Common.Services;
using Dirassati_Backend.Features.Auth.Register.Dtos;
using Dirassati_Backend.Features.Auth.Register.Services;
using FluentEmail.Core;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.Auth.SignUp
{


    [Tags("Employee Authentication")]
    [Route("api/auth")]
    [ApiController]
    [Produces("application/json")]
    public class SignUpController(RegisterService service, VerifyEmailService verifyEmailService, SendCridentialsService sendCridentialsService, IFluentEmail fluentEmail) : BaseController
    {
        private readonly RegisterService _registerService = service;
        private readonly VerifyEmailService _verifyEmailService = verifyEmailService;
        private readonly SendCridentialsService _sendCridentialsService = sendCridentialsService;

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


        [HttpGet("register/verify-email", Name = "VerifyEmail")]

        public async Task<ActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
        {

            var result = await _verifyEmailService.VerifyEmailAsync(email, token);
            if (!result.IsSuccess)
                return HandleResult(result);
            var credResult = await _sendCridentialsService.SendCridentialsAsync(email);
            return HandleResult(credResult);

        }


        [HttpGet("test-email")]
        public async Task<ActionResult> TestEmail()
        {
            try
            {
                var result = await fluentEmail.To("moh@moh.com")
                .Subject("Hoha")
                .Body("HIIIIIIIIII")
                .SendAsync();

                return !result.Successful ? BadRequest(result.ErrorMessages)
                : Ok("Email sent successfully!");

            }
            catch (System.Exception e)
            {
                throw;

            }

        }
    }
}
