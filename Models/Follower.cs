using Rediter.Api.Interfaces;

namespace Rediter.Api.Models;

public class UserFollower : IEntity
{
    public virtual Guid Id { get; set; }
    public virtual DateTime CreatedAt { get; set; }
    public virtual Guid FollowerId { get; set; }
    public virtual User Follower { get; set; } = null!;
    public virtual Guid FollowingId { get; set; }
    public virtual User Following { get; set; } = null!;

    public UserFollower() { }
}