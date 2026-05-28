using Rediter.Api.Models.Chats;
using Rediter.Api.Repositories.Chats;
using Rediter.Api.Services.UtilitariesServices;

namespace Rediter.Api.Services.Chats
{
    public class ChatParticipantService : BaseService<ChatParticipant>
    {
        private readonly ChatParticipantRepository _chatParticipantRepository;

        public ChatParticipantService(ChatParticipantRepository chatParticipantRepository) : base(chatParticipantRepository)
        {
            _chatParticipantRepository = chatParticipantRepository;
        }

        public async Task AddParticipant(Guid chatId, Guid userId)
        {
            ChatParticipant participant = new ChatParticipant
            {
                ChatId = chatId,
                UserId = userId,
            };

            _chatParticipantRepository.Insert(participant);
            await SaveChangesAsync();
        }
    }
}