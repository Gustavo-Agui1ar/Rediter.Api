using Rediter.Api.Data;
using Rediter.Api.Models.Chats;

namespace Rediter.Api.Repositories.Chats
{
    public class ChatParticipantRepository : EntityRepository<ChatParticipant>
    {
        private readonly DataContext _data;

        public ChatParticipantRepository(DataContext data) : base(data)
        {
            _data = data;
        }
    }
}