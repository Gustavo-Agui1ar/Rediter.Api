using Microsoft.EntityFrameworkCore;
using Rediter.Api.Models;
using Rediter.Api.Repositories;
using Rediter.Api.Services.UtilitariesServices;

namespace Rediter.Api.Services
{
    public class NotificationService : BaseService<Notification>
    {
        private readonly NotificationRepository _notificationRepository;

        public NotificationService(
            NotificationRepository notificationRepository
        ) : base(notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task CreatePostLikedNotification(
            Guid senderUserId,
            Guid recipientUserId,
            Guid postId,
            string senderUsername
        )
        {
            if (senderUserId == recipientUserId)
                return;

            var notification = new Notification
            {
                RecipientUserId = recipientUserId,
                SenderUserId = senderUserId,
                PostId = postId,
                Type = NotificationType.PostLiked,
                IsRead = false
            };

            _notificationRepository.Insert(notification);

            await SaveChangesAsync();
        }
    }
}