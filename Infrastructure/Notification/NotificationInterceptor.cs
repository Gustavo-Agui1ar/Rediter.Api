using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RabbitMQ.Client;
using Rediter.Api.DTOs;
using Rediter.Api.Models;
using Rediter.Api.Models.Chats;
using System.Text;
using System.Text.Json;

namespace Rediter.Api.Infrastructure.Notifications;

public class NotificationInterceptor : SaveChangesInterceptor
{
    private readonly IMediator _mediator;
    private readonly IConnection _rabbitConnection;
    private IChannel? _channel;
    private List<Notification> _pendingNotifications = new();
    private List<Message> _pendingMessages = new();
    public NotificationInterceptor(IMediator mediator, IConnection rabbitConnection)
    {
        _mediator = mediator;
        _rabbitConnection = rabbitConnection;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
     DbContextEventData eventData,
     InterceptionResult<int> result,
     CancellationToken cancellationToken = default)
    {
        if (eventData.Context == null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);

        var entitiesWithEvents = eventData.Context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entitiesWithEvents.SelectMany(e => e.DomainEvents).ToList();
        entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
            await _mediator.Publish(domainEvent, cancellationToken);

        _pendingNotifications = eventData.Context.ChangeTracker.Entries<Notification>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        var addedMessagesEntries = eventData.Context.ChangeTracker.Entries<Message>()
            .Where(e => e.State == EntityState.Added)
            .ToList();

        foreach (var entry in addedMessagesEntries)
        {
            if (entry.Entity.Sender == null && entry.Entity.SenderId != Guid.Empty)
                await entry.Reference(m => m.Sender).LoadAsync(cancellationToken);
        }

        _pendingMessages = addedMessagesEntries.Select(e => e.Entity).ToList();

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
    {
        if (eventData.Context == null)
            return await base.SavedChangesAsync(eventData, result, cancellationToken);

        if (_pendingNotifications.Any() || _pendingMessages.Any())
        {
            _channel ??= await _rabbitConnection.CreateChannelAsync(cancellationToken: cancellationToken);

            if (_pendingNotifications.Any())
            {
                foreach (var notificacao in _pendingNotifications)
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

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto));

                    await _channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: "notificacoes_fila",
                        mandatory: false,
                        basicProperties: new BasicProperties(),
                        body: body,
                        cancellationToken: cancellationToken);
                }

                _pendingNotifications.Clear();
            }

            if (_pendingMessages.Any())
            {
                foreach (var message in _pendingMessages)
                {
                    var messageDto = new MessageDTO(
                        message.Id,
                        false,
                        message.Content,
                        message.CreatedAt,
                        message.Sender.Name
                    );

                    var destinatarioId = await eventData.Context.Set<Message>()
                        .Where(m => m.Id == message.Id)
                        .SelectMany(m => m.Chat.Participants)
                        .Where(p => p.UserId != message.SenderId)
                        .Select(p => p.UserId)
                        .FirstOrDefaultAsync(cancellationToken);

                    var payload = new
                    {
                        ChatId = message.ChatId,
                        Message = messageDto,
                        ReceiverId = destinatarioId
                    };

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));

                    await _channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: "chat_mensagens_fila",
                        mandatory: false,
                        basicProperties: new BasicProperties(),
                        body: body,
                        cancellationToken: cancellationToken);
                }

                _pendingMessages.Clear();
            }
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}