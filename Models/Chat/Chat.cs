
namespace Rediter.Api.Models.Chat;

public class Chat : Entity
{
    public virtual string? Title { get; set; }
    public virtual bool IsGroup { get; set; } = false;

    public virtual ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public Chat() { }
}