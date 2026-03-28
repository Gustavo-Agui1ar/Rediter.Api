using Microsoft.AspNetCore.Mvc;
using Rediter.Api.Services;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("/Auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        public AuthController(UserService userService)
        {
            _userService = userService;
        }

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
    }
}
