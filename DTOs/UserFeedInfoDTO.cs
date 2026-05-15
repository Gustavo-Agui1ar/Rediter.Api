namespace Rediter.Api.DTOs
{
    public record UserFeedInfoDTO
    {
        public string UserName { get; init; } = string.Empty;
        public string UserID { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public bool OwnProfile { get; init; }
        public bool IsFollowing { get; init; }
        public string? ProfileImageName { get; init; }
        public string? Description { get; init; }
    }
}