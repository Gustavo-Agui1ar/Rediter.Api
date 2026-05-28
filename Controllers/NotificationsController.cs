using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rediter.Api.DTOs;
using Rediter.Api.Services.Notifications;
using System.Security.Claims;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize] // Protege a rota: apenas usuários com token JWT válido podem acessar
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(NotificationService notificationService, ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém a quantidade de notificações não lidas do usuário logado
        /// </summary>
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
                _logger.LogError(ex, "[Notifications] Erro interno ao buscar contagem de notificações para o usuário: {UserId}", GetLoggedUserIdSafe());
                return StatusCode(500, new { message = "Erro interno ao buscar contador de notificações." });
            }
        }

        /// <summary>
        /// Busca a lista de notificações do usuário com paginação baseada em cursor (Keyset Pagination)
        /// </summary>
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
                _logger.LogError(ex, "[Notifications] Erro ao buscar lista de notificações para o usuário: {UserId}", GetLoggedUserIdSafe());
                return StatusCode(500, new { message = "Erro interno ao buscar notificações." });
            }
        }

        #region Métodos Privados Auxiliares

        /// <summary>
        /// Extrai o ID do usuário do Token JWT. Lança exceção se não encontrar.
        /// </summary>
        private Guid GetLoggedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("Usuário não autenticado ou token inválido.");

            return new Guid(userIdClaim);
        }

        /// <summary>
        /// Usado apenas para logs, não quebra a aplicação se o token estiver ausente.
        /// </summary>
        private string GetLoggedUserIdSafe()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Desconhecido";
        }

        /// <summary>
        /// Marca uma notificação específica como lida
        /// </summary>
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead([FromRoute] Guid id)
        {
            try
            {
                Guid userId = GetLoggedUserId();
                await _notificationService.MarkAsReadAsync(id, userId);

                return NoContent(); // 204 No Content é o padrão para PUTs que dão certo e não retornam body
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Notifications] Erro ao marcar notificação {NotifId} como lida para o usuário: {UserId}", id, GetLoggedUserIdSafe());
                return StatusCode(500, new { message = "Erro interno ao atualizar notificação." });
            }
        }

        /// <summary>
        /// Marca todas as notificações do usuário como lidas de uma vez
        /// </summary>
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
                _logger.LogError(ex, "[Notifications] Erro ao marcar todas as notificações como lidas para o usuário: {UserId}", GetLoggedUserIdSafe());
                return StatusCode(500, new { message = "Erro interno ao atualizar notificações." });
            }
        }

        #endregion
    }
}