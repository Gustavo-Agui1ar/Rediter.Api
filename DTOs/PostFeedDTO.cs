using Org.BouncyCastle.Bcpg.OpenPgp;

namespace Rediter.Api.DTOs
{
    public class PostFeedDTO
    {
        public string Id { get; set; } = null!;
        public string? Text { get; set; }
        public string UserName { get; set; } = null!;
        public string? ImageProfileUrl { get; set; }
        public IList<string> ImageUrls { get; set; } = new List<string>();
        public string? Location { get; set; }
        public bool Edited { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public bool LikedByCurrentUser { get; set; }
        public string PostUserId { get; set; } = null!;
        public bool IsFollowing { get; set; }
        public bool OwnPost { get; set; }
    }
}
