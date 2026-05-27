using Rediter.Api.Interfaces;

namespace Rediter.Api.Models.Post;

public class PostImage : Entity
{
    public virtual Guid PostId { get; set; }
    public virtual Post? Post { get; set; }

    public virtual Guid PictureId { get; set; }
    public virtual Picture? Picture { get; set; }

    public virtual int DisplayOrder { get; set; }
    public PostImage() { }
}