using MediatR;
using Rediter.Api.Interfaces;

namespace Rediter.Api.Models;

public interface IDomainEvent : INotification { }

public abstract class Entity : IEntity
{
    public virtual Guid Id { get; set; }
    public virtual DateTime CreatedAt { get; set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}