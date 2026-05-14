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

                var posts = await _postService.GetPostsByUser(Guid.Parse(userUuid), lastCreatedAt, Guid.TryParse(lastId, out var lastIdGuid) ? lastIdGuid : (Guid?)null, pageSize, Guid.Parse(userUuid));
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar posts do usuário");
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById([FromRoute] string id)
        {
            try
            {
                string? userUuid = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userUuid == null)
                    return Unauthorized("Usuário não encontrado no token.");

                PostFeedDTO? post = await _postService.GetPostById(id, userUuid);

                if (post == null)
                    return NotFound("Post não encontrado.");

                return Ok(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar post {PostId}", id);
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

        // GET: api/posts/search?query=example&lastCreatedAt=2024-01-01T00:00:00Z&lastId=123&pageSize=10
        [HttpGet("search")]
        public async Task<IActionResult> SearchPosts([FromQuery] string query, [FromQuery] DateTime? lastCreatedAt, [FromQuery] string? lastId, [FromQuery] int pageSize, [FromQuery] bool onlyWithMedia = false)
        {
            try
            {
                var userUuid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userUuid == null)
                    return Unauthorized("Usuário não encontrado no token.");

                var posts = await _postService.SearchPosts(query, lastCreatedAt, Guid.TryParse(lastId, out var lastIdGuid) ? lastIdGuid : (Guid?)null, pageSize, onlyWithMedia, Guid.Parse(userUuid));
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar posts");
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
                var userUuid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userUuid == null)
                    return Unauthorized("Usuário não encontrado no token.");

                if (string.IsNullOrWhiteSpace(userId))
                    return BadRequest("O ID do usuário é obrigatório.");

                var posts = await _postService.GetPostsByUser(Guid.Parse(userId), lastCreatedAt, Guid.TryParse(lastId, out var lastIdGuid) ? lastIdGuid : (Guid?)null, pageSize, Guid.Parse(userUuid));
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

        //POST: api;posts/{id}/like
        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikePost([FromRoute] string id)
        {
            var userUuid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                if (userUuid == null)
                    return Unauthorized("Usuário não encontrado no token.");

                await _postService.LikePost(id, userUuid);

                return Ok(new { message = "Post liked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao curtir post {PostId}", id);
                await _postService.UnlikePost(id, userUuid!);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpDelete("{id}/like")]
        public async Task<IActionResult> UnlikePost([FromRoute] string id)
        {
            try
            {
                var userUuid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userUuid == null)
                    return Unauthorized("Usuário não encontrado no token.");
                await _postService.UnlikePost(id, userUuid);
                return Ok(new { message = "Post unliked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao descurtir post {PostId}", id);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpPost("{postId}/comments")]
        public async Task<IActionResult> PostComment([FromRoute] string postId, [FromForm] NewPostDTO dto)
        {
            try
            {
                var userUuid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userUuid == null)
                    return Unauthorized("Usuário não encontrado no token.");
                await _postService.AddComment(postId, dto, userUuid);
                return Ok(new { message = "Comentário adicionado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar comentário ao post {PostId}", postId);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }

        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetComments([FromRoute] string postId, [FromQuery] DateTime? lastCreatedAt, [FromQuery] string? lastId, [FromQuery] int pageSize)
        {
            try
            {
                var userUuid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userUuid == null)
                    return Unauthorized("Usuário não encontrado no token.");
                    
                var comments = await _postService.SearchComments(lastCreatedAt, Guid.TryParse(lastId, out var lastIdGuid) ? lastIdGuid : (Guid?)null, pageSize, Guid.Parse(postId), currentUserId: Guid.Parse(userUuid));

                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar comentário ao post {PostId}", postId);
                return StatusCode(500, "Erro interno do servidor.");
            }
        }
    }
}