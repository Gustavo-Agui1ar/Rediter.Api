
using Rediter.Api.Models.Users;

namespace Rediter.Api.Models.Chats;

public class Message : Entity
{
    public virtual Guid ChatId { get; set; }
    public virtual Chat Chat { get; set; } = null!;

    public virtual Guid? SenderId { get; set; }
    public virtual User? Sender { get; set; } = null!;

    public virtual string Content { get; set; } = null!;

    public virtual bool IsDeleted { get; set; } = false;

    public Message() { }
}