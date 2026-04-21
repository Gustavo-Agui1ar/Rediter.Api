using Microsoft.AspNetCore.Mvc;
using Rediter.Api.DTOs;
using Rediter.Api.Services;
using System.Diagnostics;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("Post")]
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
                await _postService.NewPost(dto);
                return Ok();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetPostUser")]
        public async Task<IActionResult> GetPostUser([FromQuery] string RefreshToken, [FromQuery] DateTime? lastCreatedAt, [FromQuery] string? lastId, [FromQuery] int pageSize)
        {
            try
            {
                var posts = await _postService.GetPostsByUser(RefreshToken, lastCreatedAt, lastId, pageSize);
                Debug.WriteLine("passei daqui");
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
    }
}
