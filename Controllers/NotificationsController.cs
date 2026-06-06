using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rediter.Api.DTOs;
using Rediter.Api.Services.Notifications;
using System.Security.Claims;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize] 
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;
        
        #region Métodos Privados Auxiliares

        private Guid GetLoggedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("Usuário não autenticado ou token inválido.");

            return new Guid(userIdClaim);
        }
        #endregion

        public NotificationsController(NotificationService notificationService, ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        // GET: api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                Guid userId = GetLoggedUserId();

                int count = await _notificationService.GetUnreadCountAsync(userId);

                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno ao buscar contador de notificações. \n {ex.Message}" });
            }
        }

        // GET: api/notifications?lastCreatedAt=2024-01-01T00:00:00Z&lastId=00000000-0000-0000-0000-000000000000&pageSize=15
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] DateTime? lastCreatedAt, [FromQuery] Guid? lastId, [FromQuery] int pageSize)
        {
            try
            {
                Guid userId = GetLoggedUserId();

                if (pageSize <= 0 || pageSize > 50)
                    pageSize = 15;

                var notifications = await _notificationService.GetPaginatedNotificationsAsync(userId, lastCreatedAt, lastId, pageSize);

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno ao buscar notificações. \n {ex.Message}" });
            }
        }

        // PUT: api/notifications/{id}/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead([FromRoute] Guid id)
        {
            try
            {
                Guid userId = GetLoggedUserId();
                await _notificationService.MarkAsReadAsync(id, userId);

                return NoContent(); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno ao atualizar notificação. \n {ex.Message}" });
            }
        }

        // PUT: api/notifications/read-all
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                Guid userId = GetLoggedUserId();
                await _notificationService.MarkAllAsReadAsync(userId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno ao atualizar notificações. \n {ex.Message}" });
            }
        }
    }
}