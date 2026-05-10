namespace Rediter.Api.Models;

public class UserPostLike
{
    public virtual Guid UserId { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Guid PostId { get; set; }

    public virtual Post Post { get; set; } = null!;

    public virtual DateTime CreatedAt { get; set; }
}