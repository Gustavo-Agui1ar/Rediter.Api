using Rediter.Api.Models;
using Rediter.Api.Repositories.Notifications;
using Rediter.Api.Services.UtilitariesServices;

namespace Rediter.Api.Services.Notifications
{
    public class NotificationService : BaseService<Notification>
    {
        private readonly NotificationRepository _notificationRepository;

        public NotificationService(NotificationRepository notificationRepository) : base(notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task<IList<NotificationDTO>> GetPaginatedNotificationsAsync(Guid userId, DateTime? lastCreatedAt, Guid? lastId, int pageSize)
        {
            return await _notificationRepository.GetPaginatedNotificationsAsync(userId, lastCreatedAt, lastId, pageSize);
        }

        public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            Models.Notification? notification = await _notificationRepository.GetUnreadNotificationByIdAsync(notificationId, userId);

            if (notification != null)
            {
                notification.IsRead = true;
                _notificationRepository.Update(notification);
                await SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var unreadNotifications = await _notificationRepository.GetAllUnreadNotificationsAsync(userId);

            if (unreadNotifications.Any())
            {
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    _notificationRepository.Update(notification);
                }

                await SaveChangesAsync();
            }
        }
    }
}