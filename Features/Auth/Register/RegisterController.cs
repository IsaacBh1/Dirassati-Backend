
using System.Net;
using Dirassati_Backend.Common;
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
    public class SignUpController(RegisterService registerService, VerifyEmailService verifyEmailService, SendCridentialsService sendCridentialsService) : BaseController
    {
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(EmployeeDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Resgister(RegisterDto dto)
        {
            var result = await registerService.Register(dto);
            if (result.IsSuccess)
                return Ok(result.Value);
            return BadRequest(result.Errors);
        }
        [HttpPost("register-v2")]
        public async Task<IActionResult> RegisterWithPictures([FromForm] ImprovedRegisterDto dto)
        {
            var result = await registerService.RegisterWithImageUpload(dto);
            if (result.IsSuccess)
                return Ok(result.Value);
            return BadRequest(result.Errors);
        }
        [AllowAnonymous]
        [HttpGet("register/verify-email", Name = "VerifyEmail")]
        public async Task<ActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string VerificationToken)
        {

            var result = await verifyEmailService.VerifyEmailAsync(email, VerificationToken);
            if (!result.IsSuccess)
                return HandleResult(result);
            var credResult = await sendCridentialsService.SendCridentialsAsync(email);
            return HandleResult(credResult);

        }



    }
}