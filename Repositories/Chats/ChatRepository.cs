using Microsoft.EntityFrameworkCore;
using Rediter.Api.Data;
using Rediter.Api.DTOs.Users;
using Rediter.Api.Models.Chats;

namespace Rediter.Api.Repositories.Chats
{
    public class ChatRepository : EntityRepository<Chat>
    {
        private readonly DataContext _data;

        public ChatRepository(DataContext data) : base(data)
        {
            _data = data;
        }

        public async Task<Chat?> FindChatDirect(Guid user1, Guid user2)
        {
            return await _data.Set<Chat>()
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c =>
                    !c.IsGroup &&
                    c.Participants.Any(p => p.UserId == user1) &&
                    c.Participants.Any(p => p.UserId == user2));
        }

        public async Task<List<UserChat>> GetUserChatsAsync(Guid currentUserId, DateTime? lastUpdatedAt, Guid? lastId, int pageSize)
        {
            var query = _data.Set<Chat>()
                .AsNoTracking()
                .Where(c => c.Participants.Any(p => p.UserId == currentUserId));

            if (lastUpdatedAt.HasValue && lastId.HasValue)
            {
                query = query.Where(c =>
                    c.UpdateAt < lastUpdatedAt.Value ||
                    (c.UpdateAt == lastUpdatedAt.Value && c.Id.CompareTo(lastId.Value) < 0)
                );
            }
            else if (lastUpdatedAt.HasValue) 
            {
                query = query.Where(c => c.UpdateAt < lastUpdatedAt.Value);
            }

            var chats = await query
                .OrderByDescending(c => c.UpdateAt)
                .ThenByDescending(c => c.Id) 
                .Take(pageSize)
                .Select(c => new UserChat(
                    c.Id,
                    c.UpdateAt, 

                    c.IsGroup
                        ? (c.Title ?? "Grupo sem nome")
                        : c.Participants.FirstOrDefault(p => p.UserId != currentUserId)!.User.Name
                ))
                .ToListAsync();

            return chats;
        }
    }
}