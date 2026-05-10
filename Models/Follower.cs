namespace Rediter.Api.Models;

public class UserFollower
{
    public virtual Guid FollowerId { get; set; }
    public virtual User Follower { get; set; } = null!;
    public virtual Guid FollowingId { get; set; }
    public virtual User Following { get; set; } = null!;

    public UserFollower() { }
}