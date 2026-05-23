using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Rediter.Api.Models;

namespace Rediter.Api.Infrastructure.Notification;

public class NotificationInterceptor : SaveChangesInterceptor
{
    private readonly INotificationQueue _queue;

    public NotificationInterceptor(INotificationQueue queue)
    {
        _queue = queue;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context == null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        CreateNotificationLikes(eventData.Context);
        CreateNotificationComments(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void CreateNotificationLikes(DbContext context)
    {
        var newLikes = context.ChangeTracker.Entries<UserPostLike>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        foreach (var like in newLikes)
        {
            //if (like.UserId == like.Post.UserId)
            //    continue;

            var notification = new Models.Notification
            {
                SenderUserId = like.UserId,
                RecipientUserId = like.Post.UserId,
                Type = NotificationType.PostLiked,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            context.Set<Models.Notification>().Add(notification);
        }
    }

    private static void CreateNotificationComments(DbContext context)
    {
        var newComments = context.ChangeTracker.Entries<Post>()
            .Where(e =>
                e.State == EntityState.Added &&
                e.Entity.ParentPostId != null)
            .Select(e => e.Entity)
            .ToList();

        foreach (var comment in newComments)
        {
            if (comment.ParentPost == null)
                continue;

            //if (comment.UserId == comment.ParentPost.UserId)
            //    continue;

            var notification = new Models.Notification
            {
                SenderUserId = comment.UserId,
                RecipientUserId = comment.ParentPost.UserId,
                Type = NotificationType.CommentAdded,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            context.Set<Models.Notification>().Add(notification);
        }
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context == null)
            return base.SavedChangesAsync(eventData, result, cancellationToken);

        var notifications = eventData.Context.ChangeTracker.Entries<Models.Notification>()
            .Where(e => e.State == EntityState.Unchanged)
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