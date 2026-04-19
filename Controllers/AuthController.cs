using Microsoft.AspNetCore.Mvc;
using Rediter.Api.DTOs;
using Rediter.Api.Services.UtilitariesServices;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("Auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Rediter")]
        public async Task<IActionResult> RediterAuth([FromBody]UserDTO user)
        {
            try
            {
                TokenRequestDTO token = await _authService.AuthenticateFromRediter(user.Email, user.Password);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Google")]
        public IActionResult GoogleAuth()
        {
            return Ok("Google auth successful");
        }

        [HttpGet("Code")]
        public async Task<IActionResult> VerifyCode([FromQuery] string code, [FromQuery] string userEmail)
        {
            return await GerarTokensDeAcesso(code, userEmail);
        }

        private async Task<IActionResult> GerarTokensDeAcesso(string code, string userEmail)
        {
            try
            {
                TokenRequestDTO token = await _authService.VerifyCode(code, userEmail);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while verifying the code: {ex.Message}");
            }
        }
    }
}
