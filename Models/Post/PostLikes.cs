using Rediter.Api.Infrastructure;
using Rediter.Api.Models;
using Rediter.Api.Models.Post;

public class UserPostLike : Entity 
{
    public virtual Guid UserId { get; set; }
    public virtual Guid PostId { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Post Post { get; set; } = null!;

    protected UserPostLike() { }

    public UserPostLike(Guid userId, Guid postId, Guid postOwnerId)
    {
        UserId = userId;
        PostId = postId;

        AddDomainEvent(new PostLikedEvent(userId, postOwnerId, postId));
    }
}