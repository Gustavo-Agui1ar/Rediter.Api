using Microsoft.EntityFrameworkCore;
using Rediter.Api.Data;
using Rediter.Api.DTOs;
using Rediter.Api.Models.Chats;

namespace Rediter.Api.Repositories.Chats
{
    public class MessageRepository : EntityRepository<Message>
    {
        private readonly DataContext _data;

        public MessageRepository(DataContext data) : base(data)
        {
            _data = data;
        }

        public async Task<List<MessageDTO>> GetMessagesAsync(Guid chatId, Guid currentUserId, DateTime? lastCreatedAt, Guid? lastId, int pageSize)
        {
            var query = _data.Set<Message>()
                .AsNoTracking()
                .Where(m => m.ChatId == chatId && !m.IsDeleted)
                .Where(m => m.Chat.Participants.Any(p => p.UserId == currentUserId));

            var messages = await 
                 ApplyKeysetPagination(query, lastCreatedAt, lastId)
                .OrderByDescending(m => m.CreatedAt)
                .ThenByDescending(m => m.Id)
                .Take(pageSize)
                .Select(m => new MessageDTO(
                    m.Id,
                    m.SenderId == currentUserId, 
                    m.Content,
                    m.CreatedAt
                ))
                .ToListAsync();

            return messages;
        }
    }
}