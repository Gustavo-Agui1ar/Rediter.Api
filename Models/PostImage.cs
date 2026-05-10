namespace Rediter.Api.Models;

public class PostImage
{
    public virtual Guid Id { get; set; }

    public virtual Guid PostId { get; set; }
    public virtual Post? Post { get; set; }

    public virtual int PictureId { get; set; }
    public virtual Picture? Picture { get; set; }

    public virtual int DisplayOrder { get; set; }
    public virtual DateTime CreatedAt { get; set; }

    public PostImage() { }
}