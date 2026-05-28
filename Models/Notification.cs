namespace Rediter.Api.Models;

public enum NotificationType
{
    PostLiked,
    CommentAdded,
    UserFollowed
}

public class Notification : Entity
{
    public Guid RecipientUserId { get; set; }

    public Guid SenderUserId { get; set; }

    public Guid PostId { get; set; }

    public NotificationType Type { get; set; }

    public bool IsRead { get; set; }
}