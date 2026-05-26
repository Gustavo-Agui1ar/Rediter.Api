using Google;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Rediter.Api.Data;
using Rediter.Api.Infrastructure;
using Rediter.Api.Models;

namespace Rediter.Api.Application.Handlers;

public class PostLikedEventHandler : INotificationHandler<PostLikedEvent>
{
    private readonly DataContext _context;

    public PostLikedEventHandler(DataContext context)
    {
        _context = context;
    }

    public Task Handle(PostLikedEvent notification, CancellationToken cancellationToken)
    {
        // if (notification.SenderUserId == notification.RecipientUserId) return Task.CompletedTask;

        var notifDb = new Notification
        {
            SenderUserId = notification.SenderUserId,
            RecipientUserId = notification.RecipientUserId,
            PostId = notification.PostOwner,
            Type = NotificationType.PostLiked,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
        };

        _context.Set<Notification>().Add(notifDb);

        return Task.CompletedTask;
    }
}