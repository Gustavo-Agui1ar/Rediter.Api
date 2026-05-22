using Rediter.Api.Interfaces;

namespace Rediter.Api.Models
{
    public enum NotificationType
    {
        PostLiked,
        CommentAdded,
        UserFollowed
    }

    public class Notification : IEntity
    {
        public Guid Id { get; set; }

        public Guid RecipientUserId { get; set; }

        public Guid SenderUserId { get; set; }

        public Guid PostId { get; set; }

        public NotificationType Type { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}