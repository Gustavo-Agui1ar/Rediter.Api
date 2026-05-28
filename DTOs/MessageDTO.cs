namespace Rediter.Api.DTOs
{
    public record MessageDTO(Guid messageId, bool isMine, string content, DateTime createdAt);
}
