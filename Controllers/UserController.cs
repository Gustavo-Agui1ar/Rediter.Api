using Microsoft.AspNetCore.Mvc;
using Rediter.Api.DTOs;
using Rediter.Api.Services;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("/User")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPut("Register")]
        public async Task<IActionResult> Register([FromBody] UserDTO user)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!(await _userService.CreateUser(user)))
                    return BadRequest("Failed to register user.");

                return Ok("User registered successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while registering the user: {ex.Message}");
            }
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteUser([FromBody]DeleteRequestDTO dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.UserId)) return BadRequest("User not found in DB");

                if(!( await _userService.DeleteUser(dto.UserId))) return BadRequest("Failed to delete user.");

                return Ok("User deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while deleting the user: {ex.Message}");
            }
        }
    }
}
