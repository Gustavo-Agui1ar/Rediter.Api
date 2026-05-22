using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rediter.Api.DTOs;
using Rediter.Api.Models;
using Rediter.Api.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly UserFollowerService _userFollowerService;
        private readonly UserBlockService _userBlockService;

        public UsersController(UserService userService, UserFollowerService userFollowerService, UserBlockService userBlockService)
        {
            _userService = userService;
            _userFollowerService = userFollowerService;
            _userBlockService = userBlockService;
        }
        private bool TryGetCurrentUserId(out Guid userId)
        {
            userId = Guid.Empty;
            string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrWhiteSpace(userIdStr) && Guid.TryParse(userIdStr, out userId);
        }

        // POST: api/users
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserDTO user)
        {
            try
            {
                string userId = await _userService.CreateUser(user);
                return StatusCode(201, new { userId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred while registering the user: {ex.Message}" });
            }
        }

        // GET: api/users/me
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid userId))
                    return Unauthorized(new { message = "Session Expired or Invalid User ID." });

                UserDTO? userDto = await _userService.GetUserProfileAsync(userId, userId);

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred while retrieving the user: {ex.Message}" });
            }
        }

        // GET: api/users/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUser([FromRoute] Guid id)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Session Expired" });

                UserDTO dto = await _userService.GetUserProfileAsync(id, currentUserId);

                return Ok(dto);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        // PATCH: api/users/me
        [HttpPatch("me")]
        public async Task<IActionResult> UpdateProfile([FromForm] UserUpdateDTO dto)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid userId))
                    return Unauthorized(new { message = "Session Expired or Invalid User ID." });

                User? user = await _userService.GetByGuidAsync(userId);

                if (user == null)
                    return NotFound(new { message = "User not found in DB" });

                await _userService.UpdateUserByUserDTO(dto, user);

                return Ok(new { message = "User profile updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred while updating the user profile: {ex.Message}" });
            }
        }

        // DELETE: api/users/me
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteUser()
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid userId))
                    return Unauthorized(new { message = "Session Expired or Invalid User ID." });

                if (!(await _userService.DeleteByGuidAsync(userId)))
                    return BadRequest(new { message = "Failed to delete user." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred while deleting the user: {ex.Message}" });
            }
        }

        // GET: api/users/search
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query, [FromQuery] DateTime? lastCreatedAt, [FromQuery] Guid? lastId, [FromQuery] int pageSize)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Session Expired" });

                IList<UserFeedInfoDTO> users = await _userService.SearchUsers(query, lastCreatedAt, lastId, pageSize, currentUserId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id:guid}/follow")]
        public async Task<IActionResult> FollowUser([FromRoute] Guid id)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Session Expired" });

                await _userFollowerService.FollowUser(currentUserId, id);

                return Ok(new { message = "User followed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}/unfollow")]
        public async Task<IActionResult> UnfollowUser([FromRoute] Guid id)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Session Expired" });

                await _userFollowerService.UnfollowUser(currentUserId, id);

                return Ok(new { message = "User unfollowed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id:guid}/block")]
        public async Task<IActionResult> BlockUser([FromRoute] Guid id)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Session Expired" });

                await _userBlockService.BlockUser(currentUserId, id);
                return Ok(new { message = "User blocked successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}/unblock")]
        public async Task<IActionResult> UnblockUser([FromRoute] Guid id)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Session Expired" });
                await _userBlockService.UnblockUser(currentUserId, id);
                return Ok(new { message = "User unblocked successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("me/blocked")]
        public async Task<IActionResult> GetBlockedUsers([FromQuery] DateTime? lastCreatedAt, [FromQuery] Guid? lastId, [FromQuery] int pageSize)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Session Expired" });

                var blockedUsers = await _userBlockService.GetBlockedUsers(lastCreatedAt, lastId, pageSize, currentUserId);
                return Ok(blockedUsers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}