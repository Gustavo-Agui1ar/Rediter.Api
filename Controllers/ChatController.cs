using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rediter.Api.DTOs; 
using Rediter.Api.DTOs.Users;
using Rediter.Api.Services.Chats;
using System.Security.Claims;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("api/chats")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;
        private readonly MessageService _messageService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            ChatService chatService,
            MessageService messageService,
            ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _messageService = messageService;
            _logger = logger;
        }

        private bool TryGetCurrentUserId(out Guid userId)
        {
            userId = Guid.Empty;
            string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrWhiteSpace(userIdStr) && Guid.TryParse(userIdStr, out userId);
        }

        // POST: api/chats/direct/{receiverId}
        [HttpPost("direct/{receiverId:guid}")]
        public async Task<IActionResult> GetOrCreateDirectChat([FromRoute] Guid receiverId, [FromBody] NewMessageDTO dto)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Usuário não encontrado no token." });

                Guid chatId = await _chatService.GetOrCreateDirectChatAsync(currentUserId, receiverId);

                MessageDTO createdMessage = await _chatService.AddMessageToChat(chatId, currentUserId, dto.Content);

                return Ok(new
                {
                    chatId = chatId,
                    messageId = createdMessage.messageId,
                    createdAt = createdMessage.createdAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter ou criar chat direto com o usuário {ReceiverId}", receiverId);
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // POST: api/chats/{chatId}/messages
        [HttpPost("{chatId:guid}/messages")]
        public async Task<IActionResult> SendMessage([FromRoute] Guid chatId, [FromBody] NewMessageDTO dto)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Usuário não encontrado no token." });

                if (string.IsNullOrWhiteSpace(dto.Content))
                    return BadRequest(new { message = "O conteúdo da mensagem não pode estar vazio." });

                MessageDTO createdMessage = await _chatService.AddMessageToChat(chatId, currentUserId, dto.Content);

                return Ok(createdMessage);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar mensagem no chat {ChatId}", chatId);
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // GET: api/chats
        [HttpGet]
        public async Task<IActionResult> GetMyChats([FromQuery] DateTime? lastUpdatedAt, [FromQuery] Guid? lastId, [FromQuery] int pageSize = 20)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Usuário não encontrado no token." });

                List<UserChat> chats = await _chatService.GetUserChatsAsync(currentUserId, lastUpdatedAt, lastId, pageSize);

                return Ok(chats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar a lista de chats do usuário");
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        // GET: api/chats/{chatId}/messages
        [HttpGet("{chatId:guid}/messages")]
        public async Task<IActionResult> GetChatMessages([FromRoute] Guid chatId, [FromQuery] DateTime? lastCreatedAt, [FromQuery] Guid? lastId, [FromQuery] int pageSize = 50)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Usuário não encontrado no token." });

                List<MessageDTO> messages = await _messageService.GetMessagesAsync(chatId, currentUserId, lastCreatedAt, lastId, pageSize);

                return Ok(messages);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid(); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar mensagens do chat {ChatId}", chatId);
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [HttpPost("group")]
        public async Task<IActionResult> CreateGroupMessage([FromBody] CreateGroupDTO group)
        {
            try
            {
                if (!TryGetCurrentUserId(out Guid currentUserId))
                    return Unauthorized(new { message = "Usuário não encontrado no token." });

                Guid chatId = await _chatService.CreateGroupChatAsync(currentUserId, group.Title, group.ParticipantsIds);
                return Ok(new { chatId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar chat em grupo com o título {GroupTitle}", group.Title);
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }
    }
}