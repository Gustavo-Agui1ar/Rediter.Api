using Microsoft.AspNetCore.Mvc;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("/Auth")]
    public class AuthController : ControllerBase
    {
        [HttpPost("Rediter")]
        public IActionResult RediterAuth()
        {
            return Ok("Rediter auth successful");
        }

        [HttpPost("Google")]
        public IActionResult GoogleAuth()
        {
            return Ok("Google auth successful");
        }

        [HttpPut("Register")]
        public IActionResult Register()
        {
            return Ok("User registered successfully");
        }
    }
}
