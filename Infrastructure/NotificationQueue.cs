using Rediter.Api.Models;
using System.Collections.Concurrent;

public class NotificationQueue : INotificationQueue
{
    private readonly ConcurrentQueue<(Guid userId, NotificationDTO dto)> _queue = new();

    public void Enqueue(NotificationDTO dto, Guid recipientId)
    {
        _queue.Enqueue((recipientId, dto));
    }

    public bool TryDequeue(out (Guid userId, NotificationDTO dto) item)
    {
        return _queue.TryDequeue(out item);
    }
}