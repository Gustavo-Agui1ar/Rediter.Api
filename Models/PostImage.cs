using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rediter.Api.Models
{
    [Table("post_images")]
    public class PostImage
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("post_id")]
        public Guid PostId { get; set; }

        [ForeignKey(nameof(PostId))]
        public virtual Post? Post { get; set; }

        [Column("picture_id")]
        public int PictureId { get; set; }

        [ForeignKey(nameof(PictureId))]
        public virtual Picture? Picture { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public PostImage() { }
    }
}