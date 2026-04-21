using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rediter.Api.DTOs;
using Rediter.Api.Models;
using Rediter.Api.Services;
using System.Security.Claims;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("User")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPut("Register")]
        public async Task<IActionResult> Register([FromBody] UserDTO user)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string userId = await _userService.CreateUser(user);

                return Ok(new { userId = userId });
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while registering the user: {ex.Message}");
            }
        }

        [HttpDelete("DeleteAccount")]
        public async Task<IActionResult> DeleteUser()
        {
            try
            {
                string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userIdStr)) return BadRequest("User not found in DB");

                if (!(await _userService.DeleteByGuidAsync(new Guid(userIdStr)))) return BadRequest("Failed to delete user.");

                return Ok("User deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while deleting the user: {ex.Message}");
            }
        }

        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                User? user = await _userService.GetByGuidAsync(new Guid(userIdStr!));

                if (user == null) return BadRequest("User not found in DB");

                UserDTO? userDto = await _userService.GetUserDto(user);

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while retrieving the user: {ex.Message}");
            }
        }

        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UserUpdateDTO dto)
        {
            try
            {
                string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                User? user = await _userService.GetByGuidAsync(new Guid(userIdStr!));
               
                if (user == null) return BadRequest("User not found in DB");

                await _userService.UpdateUserByUserDTO(dto, user);
                return Ok("User profile updated successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while updating the user profile: {ex.Message}");
            }
        }
    }
}
