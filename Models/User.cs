using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rediter.Api.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;
        
        [Column("email")]
        public string Email { get; set; } = null!;
        
        [Column("password")]
        public string Password { get; set; } = null!;
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("profile_picture_id")]
        public int? ProfilePictureId { get; set; }

        [ForeignKey(nameof(ProfilePictureId))]
        public virtual Picture? ProfilePicture { get; set; }

        public User() { }
    }
}
