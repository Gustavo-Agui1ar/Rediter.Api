using Rediter.Api.Models;

public interface INotificationQueue
{
    void Enqueue(NotificationDTO dto, Guid recipientId);
}