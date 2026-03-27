using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Rediter.Api.Models
{
    [Table("users")]
    public class User : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = null!;

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

        public User() { }
    }
}
