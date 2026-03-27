using Microsoft.AspNetCore.Mvc;
using Rediter.Api.DTOs;
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

        [HttpPut("Register")]
        public async Task<IActionResult> Register([FromBody]UserDTO user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if(!(await _userService.CreateUser(user)))
                return BadRequest("Failed to register user.");

            return Ok("User registered successfully");
        }

        [HttpDelete("Delete")]
        public IActionResult DeleteUser(string userId)
        {
            if(!_userService.DeleteUser(userId)) { return BadRequest("Failed to delete user."); }

            return Ok("User deleted successfully");
        }
    }
}
