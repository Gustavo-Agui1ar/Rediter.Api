
using Rediter.Api.Models.Users;

namespace Rediter.Api.Models.Posts;

public class Post : Entity
{
    public virtual Guid UserId { get; set; }
    public virtual User? User { get; set; }
    public virtual Guid? ParentPostId { get; set; }
    public virtual Post? ParentPost { get; set; }
    public virtual int CommentsCount { get; set; } = 0;
    public virtual ICollection<Post> Replies { get; set; } = new List<Post>();
    public virtual string? LocationName { get; set; }
    public virtual string? Content { get; set; }
    public virtual DateTime UpdatedAt { get; set; }
    public virtual int LikesCount { get; set; }
    public virtual ICollection<PostImage> PostImages { get; set; } = new List<PostImage>();
    public virtual ICollection<UserPostLike> Likes { get; set; } = new List<UserPostLike>();
    public Post() { }
}