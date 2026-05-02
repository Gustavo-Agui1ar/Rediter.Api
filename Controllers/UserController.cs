using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rediter.Api.DTOs;
using Rediter.Api.Models;
using Rediter.Api.Services;
using System.Security.Claims;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("api/users")] 
    [Authorize]
    public class UsersController : ControllerBase 
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        // POST: api/users
        [AllowAnonymous]
        [HttpPost] 
        public async Task<IActionResult> Register([FromBody] UserDTO user)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string userId = await _userService.CreateUser(user);
                return StatusCode(201, new { userId = userId });
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while registering the user: {ex.Message}");
            }
        }

        // GET: api/users/me
        [HttpGet("me")] 
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userIdStr))
                    return Unauthorized("User ID claim not found.");

                User? user = await _userService.GetByGuidAsync(new Guid(userIdStr));

                if (user == null)
                    return NotFound("User not found in DB"); 

                UserDTO? userDto = await _userService.GetUserDto(user);

                return Ok(userDto); 
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while retrieving the user: {ex.Message}");
            }
        }

        // PATCH: api/users/me
        [HttpPatch("me")] 
        public async Task<IActionResult> UpdateProfile([FromForm] UserUpdateDTO dto)
        {
            try
            {
                string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userIdStr))
                    return Unauthorized("User ID claim not found.");

                User? user = await _userService.GetByGuidAsync(new Guid(userIdStr));

                if (user == null)
                    return NotFound("User not found in DB");

                await _userService.UpdateUserByUserDTO(dto, user);

                return Ok(new { message = "User profile updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while updating the user profile: {ex.Message}");
            }
        }

        // DELETE: api/users/me
        [HttpDelete("me")] 
        public async Task<IActionResult> DeleteUser()
        {
            try
            {
                string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userIdStr))
                    return Unauthorized("User ID claim not found.");

                if (!(await _userService.DeleteByGuidAsync(new Guid(userIdStr))))
                    return BadRequest("Failed to delete user.");

                return NoContent(); 
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while deleting the user: {ex.Message}");
            }
        }
    }
}