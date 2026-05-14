namespace Rediter.Api.Models;

public class UserPostLike : IEntity
{
    public virtual Guid Id { get; set; }
    public virtual Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Guid PostId { get; set; }

    public virtual Post Post { get; set; } = null!;

    public virtual DateTime CreatedAt { get; set; }
}