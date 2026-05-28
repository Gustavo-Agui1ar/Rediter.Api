using Rediter.Api.DTOs.Users;
using Rediter.Api.Models.Chats;
using Rediter.Api.Repositories.Chats;
using Rediter.Api.Services.UtilitariesServices;


namespace Rediter.Api.Services.Chats
{
    public class ChatService : BaseService<Chat>
    {
        private readonly ChatRepository _chatRepository;

        public ChatService(ChatRepository chatRepository) : base(chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<Guid> GetOrCreateDirectChatAsync(Guid userId1, Guid userId2)
        {
            Chat? existingChat = await _chatRepository.FindChatDirect(userId1, userId2);

            if (existingChat != null)
                return existingChat.Id;

            Chat newChat = new Chat
            {
                Title = null,
                IsGroup = false,
                Participants = new List<ChatParticipant>
                {
                    new ChatParticipant { UserId = userId1 },
                    new ChatParticipant { UserId = userId2 }
                }
            };

            _chatRepository.Insert(newChat);

            await SaveChangesAsync();

            return newChat.Id;
        }

        public async Task AddMessageToChat(Guid chatId, Guid senderId, string content)
        {
            Chat? chat = await _chatRepository.GetByUuid(chatId);
            
            if (chat == null)
                throw new Exception("Chat not found");
            
            Message message = new Message
            {
                ChatId = chatId,
                SenderId = senderId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            chat.UpdateAt = DateTime.UtcNow;
            chat.Messages.Add(message);
            _chatRepository.Update(chat);
            
            await SaveChangesAsync();
        }

        public async Task<List<UserChat>> GetUserChatsAsync(Guid currentUserId, DateTime? lastUpdatedAt, Guid? lastId, int pageSize)
        {
            return await _chatRepository.GetUserChatsAsync(currentUserId, lastUpdatedAt, lastId, pageSize);
        }
    }
}