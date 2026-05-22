namespace Rediter.Api.Models
{
    public record NotificationDTO
    {
        public Guid Id { get; init; }
        public Guid SenderUserId { get; init; }
        public Guid ReceiverUserId { get; init; }
        public string SenderUsername { get; set; } = null!;
        public string Type { get; init; } = null!;
        public bool IsRead { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}