using Rediter.Api.Models;

public interface INotificationDispatcher
{
    Task DispatchToUserAsync(Guid userId, NotificationDTO notificationDto);
}