using Microsoft.EntityFrameworkCore;
using Rediter.Api.Data;
using Rediter.Api.DTOs;
using Rediter.Api.Models;

namespace Rediter.Api.Repositories
{
    public class NotificationRepository : BaseRepository<Notification>
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

        public async Task<Notification?> GetUnreadNotificationByIdAsync(Guid notificationId, Guid userId)
        {
            return await _data.Set<Notification>()
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientUserId == userId && !n.IsRead);
        }

        public async Task<List<Notification>> GetAllUnreadNotificationsAsync(Guid userId)
        {
            return await _data.Set<Notification>()
                .Where(n => n.RecipientUserId == userId && !n.IsRead)
                .ToListAsync();
        }
    }
}