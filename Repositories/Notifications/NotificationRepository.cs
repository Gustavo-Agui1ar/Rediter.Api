using Microsoft.EntityFrameworkCore;
using Rediter.Api.Data;
using Rediter.Api.Models;
using Rediter.Api.Models.Users;

namespace Rediter.Api.Repositories.Notifications
{
    public class NotificationRepository : EntityRepository<Notification>
    {
        private readonly DataContext _data;

        public NotificationRepository(DataContext data) : base(data)
        {
            _data = data;
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _data.Set<Notification>()
                .Where(n => n.RecipientUserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task<IList<NotificationDTO>> GetPaginatedNotificationsAsync(Guid userId, DateTime? lastCreatedAt, Guid? lastId, int pageSize)
        {
            var query = _data.Set<Notification>()
                .Where(n => n.RecipientUserId == userId);

            query = ApplyKeysetPagination(query, lastCreatedAt, lastId);
            
            var pagedNotifications = query
                .OrderByDescending(n => n.CreatedAt)
                .ThenByDescending(n => n.Id)
                .Take(pageSize);

            var result = await (from n in pagedNotifications
                                join u in _data.Set<User>() on n.SenderUserId equals u.Id
                                select new NotificationDTO
                                {
                                    Id = n.Id,
                                    SenderUserId = n.SenderUserId,
                                    SenderUsername = u.Name,
                                    Type = n.Type.ToString(),
                                    IsRead = n.IsRead,
                                    CreatedAt = n.CreatedAt
                                }).ToListAsync();

            return result;
        }

        public async Task<Models.Notification?> GetUnreadNotificationByIdAsync(Guid notificationId, Guid userId)
        {
            return await _data.Set<Models.Notification>()
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientUserId == userId && !n.IsRead);
        }

        public async Task<List<Models.Notification>> GetAllUnreadNotificationsAsync(Guid userId)
        {
            return await _data.Set<Models.Notification>()
                .Where(n => n.RecipientUserId == userId && !n.IsRead)
                .ToListAsync();
        }
    }
}