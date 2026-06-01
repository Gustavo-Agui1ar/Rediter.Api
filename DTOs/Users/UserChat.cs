namespace Rediter.Api.DTOs.Users
{
    public record UserChat(
        Guid ChatId,
        string TargetUserId,
        string TitleChat,
        string? TargetUserImage,
        string LastMessageContent,
        DateTime LastUpdatedAt,
        int? UnreadCount
    );
}