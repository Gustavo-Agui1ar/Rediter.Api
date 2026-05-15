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

        // Helper para obter e validar o ID do usuário logado sem lançar exceções
        private bool TryGetCurrentUserId(out Guid userId)
        {
            userId = Guid.Empty;
            string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrWhiteSpace(userIdStr) && Guid.TryParse(userIdStr, out userId);
        }

        // POST: api/posts
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] NewPostDTO dto)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Usuário não encontrado no token ou token inválido." });

                await _postService.NewPost(dto, currentUserId);

                return Created(string.Empty, new { message = "Post criado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar post");
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // GET: api/posts/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyPosts([FromQuery] DateTime? lastCreatedAt, [FromQuery] Guid? lastId, [FromQuery] int pageSize)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Usuário não encontrado no token." });

                var posts = await _postService.GetPostsByUser(currentUserId, lastCreatedAt, lastId, pageSize, currentUserId);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar posts do usuário");
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // GET: api/posts/{id}
        // OTIMIZAÇÃO: Rota restrita a Guid para validação automática
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPostById([FromRoute] Guid id)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Usuário não encontrado no token." });

                PostFeedDTO? post = await _postService.GetPostById(id, currentUserId);

                if (post == null)
                    return NotFound(new { message = "Post não encontrado." });

                return Ok(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar post {PostId}", id);
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // PUT: api/posts/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdatePost([FromRoute] Guid id, [FromForm] UpdatePostDTO dto)
        {
            try
            {
                // Apenas como boa prática, garantir que quem altera está logado
                if (!TryGetCurrentUserId(out _))
                    return Unauthorized(new { message = "Usuário não encontrado no token." });

                await _postService.UpdatePost(dto, id.ToString());

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar post {PostId}", id);
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // GET: api/posts/search
        [HttpGet("search")]
        public async Task<IActionResult> SearchPosts([FromQuery] string query, [FromQuery] DateTime? lastCreatedAt, [FromQuery] Guid? lastId, [FromQuery] int pageSize, [FromQuery] bool onlyWithMedia = false)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Usuário não encontrado no token." });

                var posts = await _postService.SearchPosts(query, lastCreatedAt, lastId, pageSize, onlyWithMedia, currentUserId);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar posts");
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // DELETE: api/posts/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeletePost([FromRoute] Guid id)
        {
            try
            {
                if (!TryGetCurrentUserId(out _))
                    return Unauthorized(new { message = "Usuário não encontrado no token." });

                await _postService.DeletePost(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar post {PostId}", id);
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // GET: api/posts/me/media
        [HttpGet("me/media")]
        public async Task<IActionResult> GetMyMediaNames()
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Usuário não encontrado no token." });

                IList<string> mediaNames = await _postService.GetAllMidiaNames(currentUserId);
                return Ok(mediaNames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar mídias do usuário");
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // GET: api/posts/user/{userId}
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetUserPosts([FromRoute] Guid userId, [FromQuery] DateTime? lastCreatedAt, [FromQuery] Guid? lastId, [FromQuery] int pageSize)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Usuário não encontrado no token." });

                var posts = await _postService.GetPostsByUser(userId, lastCreatedAt, lastId, pageSize, currentUserId);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar posts do usuário {UserId}", userId);
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // GET: api/posts/user/{userId}/media
        [HttpGet("user/{userId:guid}/media")]
        public async Task<IActionResult> GetUserMediaNames([FromRoute] Guid userId)
        {
            try
            {
                if (!TryGetCurrentUserId(out _))
                    return Unauthorized(new { message = "Usuário não encontrado no token." });

                IList<string> mediaNames = await _postService.GetAllMidiaNames(userId);
                return Ok(mediaNames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar mídias do usuário {UserId}", userId);
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // POST: api/posts/{id}/like
        [HttpPost("{id:guid}/like")]
        public async Task<IActionResult> LikePost([FromRoute] Guid id)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Usuário não encontrado no token." });

                await _postService.LikePost(id, currentUserId);

                return Ok(new { message = "Post liked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao curtir post {PostId}", id);
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // DELETE: api/posts/{id}/like
        [HttpDelete("{id:guid}/like")]
        public async Task<IActionResult> UnlikePost([FromRoute] Guid id)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Usuário não encontrado no token." });

                await _postService.UnlikePost(id, currentUserId);

                return Ok(new { message = "Post unliked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao descurtir post {PostId}", id);
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // POST: api/posts/{postId}/comments
        [HttpPost("{postId:guid}/comments")]
        public async Task<IActionResult> PostComment([FromRoute] Guid postId, [FromForm] NewPostDTO dto)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Usuário não encontrado no token." });

                await _postService.AddComment(postId, dto, currentUserId);

                return Ok(new { message = "Comentário adicionado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar comentário ao post {PostId}", postId);
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // GET: api/posts/{postId}/comments
        [HttpGet("{postId:guid}/comments")]
        public async Task<IActionResult> GetComments([FromRoute] Guid postId, [FromQuery] DateTime? lastCreatedAt, [FromQuery] Guid? lastId, [FromQuery] int pageSize)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Usuário não encontrado no token." });

                var comments = await _postService.SearchComments(lastCreatedAt, lastId, pageSize, postId, currentUserId: currentUserId);

                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar comentários do post {PostId}", postId);
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }
    }
}