using Rediter.Api.Interfaces;

namespace Rediter.Api.Models;

public class UserBlock : IEntity
{
    public virtual Guid Id { get; set; }
    public virtual DateTime CreatedAt { get; set; }

    public virtual Guid BlockerId { get; set; }
    public virtual User Blocker { get; set; } = null!;

    public virtual Guid BlockedId { get; set; }
    public virtual User Blocked { get; set; } = null!;

    public UserBlock() { }
}