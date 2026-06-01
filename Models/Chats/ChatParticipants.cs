using Rediter.Api.Models.Users;

namespace Rediter.Api.Models.Chats;

public class ChatParticipant : Entity
{
    public virtual Guid ChatId { get; set; }
    public virtual Chat Chat { get; set; } = null!;
    public virtual Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public virtual bool IsMuted { get; set; } = false;
    public virtual DateTime? LastReadAt { get; set; }
    public ChatParticipant() { }
}