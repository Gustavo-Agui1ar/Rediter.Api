using Rediter.Api.Interfaces;

namespace Rediter.Api.Models.Chat;

public class ChatParticipant : Entity
{
    public virtual Guid ChatId { get; set; }
    public virtual Chat Chat { get; set; } = null!;

    public virtual Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public virtual bool IsMuted { get; set; } = false;

    public ChatParticipant() { }
}