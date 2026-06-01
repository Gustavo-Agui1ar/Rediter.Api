namespace Rediter.Api.Models.Users;

public class Role : Entity
{
    public virtual string Name { get; set; } = null!;
    public virtual string Description { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public Role() { }
}