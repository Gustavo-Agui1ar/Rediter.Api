namespace Rediter.Api.DTOs
{
    public class UserFeedInfoDTO
    {
        public string UserName { get; set; } = null!;
        public string UserID { get; set; } = string.Empty!;
        public string? ProfileImageName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Description { get; set; }
        public bool OwnProfile { get; set; }
        public bool IsFollowing { get; set; }

    }
}
