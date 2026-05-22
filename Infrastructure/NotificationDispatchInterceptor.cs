using Microsoft.EntityFrameworkCore.Diagnostics;
using Rediter.Api.Models;
using Rediter.Api.Interfaces;
using Rediter.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Rediter.Api.Data.Interceptors
{
   public class NotificationDispatchInterceptor : SaveChangesInterceptor
{
    private readonly INotificationQueue _queue;

    public NotificationDispatchInterceptor(INotificationQueue queue)
    {
        _queue = queue;
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context == null)
            return base.SavedChangesAsync(eventData, result, cancellationToken);

        var notifications = eventData.Context
            .ChangeTracker
            .Entries<Notification>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

            foreach (var notificacao in notifications)
        {
            NotificationDTO dto = new()
            {
                Id = notificacao.Id,
                SenderUserId = notificacao.SenderUserId,
                ReceiverUserId = notificacao.RecipientUserId,
                Type = notificacao.Type.ToString(),
                IsRead = notificacao.IsRead,
                CreatedAt = notificacao.CreatedAt
            };

            _queue.Enqueue(dto, notificacao.RecipientUserId);
        }

        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}
}