namespace Rediter.Api.Models.Users;

public class Follower : Entity
{
    public Guid FollowerId { get; set; }
    public User UserFollower { get; set; } = null!;

    public Guid FollowingId { get; set; }
    public User UserFollowing { get; set; } = null!;

    public Follower() { }
}