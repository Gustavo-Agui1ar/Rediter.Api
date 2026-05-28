using Rediter.Api.Interfaces;
using Rediter.Api.Models.Chats;

namespace Rediter.Api.Models.Users;

public class User : Entity
{
    public virtual string Name { get; set; } = null!;
    public virtual string Email { get; set; } = null!;
    public virtual string? Password { get; set; }

    public virtual Guid? ProfilePictureId { get; set; }
    public virtual Picture? ProfilePicture { get; set; }

    public virtual Guid? ProfileCoverId { get; set; }
    public virtual Picture? ProfileCover { get; set; }

    public virtual bool IsVerified { get; set; }
    public virtual string? VerificationCode { get; set; }
    public virtual string? RefreshToken { get; set; }
    public virtual DateTime? RefreshTokenExpiration { get; set; }
    public virtual string? Description { get; set; }

    public virtual string? DeviceToken { get; set; } 

    public virtual int FollowersCount { get; set; } = 0;
    public virtual int FollowingCount { get; set; } = 0;

    public virtual ICollection<Follower> Followers { get; set; } = new List<Follower>();
    public virtual ICollection<Follower> Following { get; set; } = new List<Follower>();

    public virtual ICollection<UserBlock> BlockedUsers { get; set; } = new List<UserBlock>();
    public virtual ICollection<UserBlock> BlockedBy { get; set; } = new List<UserBlock>();

    public virtual ICollection<ChatParticipant> Chats { get; set; } = new List<ChatParticipant>();

    public User() { }
}