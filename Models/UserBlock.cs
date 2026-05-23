using Rediter.Api.Interfaces;

namespace Rediter.Api.Models;

public class UserBlock : Entity
{
    public virtual Guid BlockerId { get; set; }
    public virtual User Blocker { get; set; } = null!;

    public virtual Guid BlockedId { get; set; }
    public virtual User Blocked { get; set; } = null!;

    public UserBlock() { }
}