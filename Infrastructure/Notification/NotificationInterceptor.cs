using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RabbitMQ.Client;
using Rediter.Api.Models;

namespace Rediter.Api.Infrastructure.Notification;

public class NotificationInterceptor : SaveChangesInterceptor
{
    private readonly IMediator _mediator;
    private readonly IConnection _rabbitConnection; 

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

        var domainEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
            await _mediator.Publish(domainEvent, cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context == null)
            return await base.SavedChangesAsync(eventData, result, cancellationToken);

        var notifications = eventData.Context.ChangeTracker.Entries<Models.Notification>()
            .Where(e => e.State == EntityState.Unchanged) 
            .Select(e => e.Entity)
            .ToList();

        if (notifications.Any())
        {
            await using var channel = await _rabbitConnection.CreateChannelAsync(cancellationToken: cancellationToken);

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

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto));

                await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: "notificacoes_fila", 
                    mandatory: false,
                    basicProperties: new BasicProperties(),
                    body: body,
                    cancellationToken: cancellationToken);
            }
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}