namespace Rediter.Api.DTOs.Posts
{
    public record PostFeedDTO
    {
        public string Id { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string? ProfileImageName { get; init; }
        public string? Text { get; init; }
        public string? Location { get; init; }
        public IList<string> ImageUrls { get; init; } = new List<string>();
        public bool Edited { get; init; }
        public DateTime CreatedAt { get; init; }
        public int LikesCount { get; init; }
        public int CommentsCount { get; init; }
        public bool LikedByCurrentUser { get; init; }
        public string PostUserID { get; init; } = string.Empty;
        public bool IsFollowing { get; init; }
        public bool OwnPost { get; init; }
        public int Score { get; init; }
    }
}