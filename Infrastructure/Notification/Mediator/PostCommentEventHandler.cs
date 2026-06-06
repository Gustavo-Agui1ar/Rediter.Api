using MediatR;
using Rediter.Api.Data;
using Rediter.Api.Infrastructure;
using Rediter.Api.Models;

namespace Rediter.Api.Application.Handlers;

public class PostCommentEventHandler : INotificationHandler<CommentAddedEvent>
{
    private readonly DataContext _context;

    public PostCommentEventHandler(DataContext context)
    {
        _context = context;
    }

    public Task Handle(CommentAddedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.SenderUserId == notification.RecipientUserId) return Task.CompletedTask;

        var notifDb = new Notification
        {
            SenderUserId = notification.SenderUserId,
            RecipientUserId = notification.RecipientUserId,
            Type = NotificationType.CommentAdded,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<Notification>().Add(notifDb);

        return Task.CompletedTask;
    }
}