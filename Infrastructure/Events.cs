using Rediter.Api.Models;

namespace Rediter.Api.Infrastructure;

public record PostLikedEvent(Guid SenderUserId, Guid RecipientUserId, Guid PostOwner) : IDomainEvent;
public record CommentAddedEvent(Guid SenderUserId, Guid RecipientUserId) : IDomainEvent;