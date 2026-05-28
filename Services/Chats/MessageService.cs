using Rediter.Api.DTOs;
using Rediter.Api.Models.Chats;
using Rediter.Api.Repositories.Chats;
using Rediter.Api.Services.UtilitariesServices;

namespace Rediter.Api.Services.Chats
{
    public class MessageService : BaseService<Message>
    {
        private readonly MessageRepository _messageRepository;

        public MessageService(MessageRepository messageRepository) : base(messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task<List<MessageDTO>> GetMessagesAsync(Guid chatId, Guid currentUserId, DateTime? lastCreatedAt, Guid? lastId, int pageSize)
        {
            return await _messageRepository.GetMessagesAsync(chatId, currentUserId, lastCreatedAt, lastId, pageSize);
        }
    }
}