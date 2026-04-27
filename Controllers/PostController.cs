using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rediter.Api.DTOs;
using Rediter.Api.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("Post")]
    [Authorize]
    public class PostController : ControllerBase
    {
        private readonly PostService _postService;
        public PostController(PostService postService)
        {
            _postService = postService;
        }

        [HttpPost("NewPost")]
        public async Task<IActionResult> NewPost([FromForm] NewPostDTO dto)
        {
            try
            {
                string? userUuid = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if(userUuid == null) 
                    return BadRequest("User not found in token");

                await _postService.NewPost(dto, userUuid);
                return Ok("Post created successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetPostUser")]
        public async Task<IActionResult> GetPostUser([FromQuery] DateTime? lastCreatedAt, [FromQuery] string? lastId, [FromQuery] int pageSize)
        {
            try
            {
                string? userUuid = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if(userUuid == null) 
                    return BadRequest("User not found in token");

                var posts = await _postService.GetPostsByUser(userUuid, lastCreatedAt, lastId, pageSize);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdatePost/{postId}")]
        public async Task<IActionResult> UpdatePost([FromForm] UpdatePostDTO dto, [FromRoute] string postId)
        {
            try
            {
                await _postService.UpdatePost(dto, postId);
                return Ok();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeletePost/{postId}")]
        public async Task<IActionResult> DeletePost([FromRoute] string postId)
        {
            try
            {
                await _postService.DeletePost(postId);
                return Ok();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetMyMidiaNames")]
        public async Task<IActionResult> GetMyMidiaNames()
        {
            try
            {
                var userUuid = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if(userUuid == null)
                    return BadRequest("User not found in token");

                IList<string> midias = await _postService.GetAllMidiaNames(userUuid);
                return Ok(midias);

            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
