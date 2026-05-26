using MediatR;
using Rediter.Api.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rediter.Api.Models;

public interface IDomainEvent : INotification { }

public abstract class Entity : IEntity
{
    public virtual Guid Id { get; set; }
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    private readonly List<IDomainEvent> _domainEvents = new();
    
    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}