namespace Rediter.Api.DTOs.Users
{
    public record UserChat(Guid chatId, DateTime createdAt, string title);
}
