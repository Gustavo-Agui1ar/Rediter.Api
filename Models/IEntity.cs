namespace Rediter.Api.Models
{
    public interface IEntity
    {
        Guid Id { get; set; }
        DateTime CreatedAt { get; set; }
    }
}
