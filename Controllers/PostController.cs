using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rediter.Api.DTOs;
using Rediter.Api.Services;
using System.Security.Claims;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("api/posts")]
    [Authorize]
    public class PostController : ControllerBase
    {
        private readonly PostService _postService;
        private readonly ILogger<PostController> _logger;

        public PostController(PostService postService, ILogger<PostController> logger)
        {
            _postService = postService;
            _logger = logger;
        }

        // POST: api/posts
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] NewPostDTO dto)
        {
            try
            {
                var userUuid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userUuid == null)
                    return Unauthorized("Usuário não encontrado no token.");

                await _postService.NewPost(dto, userUuid);

                return Created(string.Empty, new { message = "Post criado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar post");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        // GET: api/posts/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyPosts([FromQuery] DateTime? lastCreatedAt, [FromQuery] string? lastId, [FromQuery] int pageSize)
        {
            try
            {
                var userUuid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userUuid == null)
                    return Unauthorized("Usuário não encontrado no token.");

                var posts = await _postService.GetPostsByUser(userUuid, lastCreatedAt, lastId, pageSize);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar posts do usuário");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        // PUT: api/posts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost([FromRoute] string id, [FromForm] UpdatePostDTO dto)
        {
            try
            {
                await _postService.UpdatePost(dto, id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar post {PostId}", id);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        // DELETE: api/posts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost([FromRoute] string id)
        {
            try
            {
                await _postService.DeletePost(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar post {PostId}", id);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpGet("me/media")]
        public async Task<IActionResult> GetMyMediaNames()
        {
            try
            {
                var userUuid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userUuid == null)
                    return Unauthorized("Usuário não encontrado no token.");

                IList<string> mediaNames = await _postService.GetAllMidiaNames(userUuid);
                return Ok(mediaNames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar mídias do usuário");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        // GET: api/posts/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserPosts([FromRoute] string userId, [FromQuery] DateTime? lastCreatedAt, [FromQuery] string? lastId, [FromQuery] int pageSize)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return BadRequest("O ID do usuário é obrigatório.");
                var posts = await _postService.GetPostsByUser(userId, lastCreatedAt, lastId, pageSize);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar posts do usuário {UserId}", userId);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        // GET: api/posts/user/{userId}/media
        [HttpGet("user/{userId}/media")]
        public async Task<IActionResult> GetUserMediaNames([FromRoute] string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return BadRequest("O ID do usuário é obrigatório.");

                IList<string> mediaNames = await _postService.GetAllMidiaNames(userId);
                return Ok(mediaNames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar mídias do usuário {UserId}", userId);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }
    }
}