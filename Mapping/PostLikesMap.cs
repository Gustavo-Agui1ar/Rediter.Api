using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Data.Mappings;
using Rediter.Api.Models;

namespace Rediter.Api.Mappings
{
    public class UserPostLikeMap : EntityMap<UserPostLike>
    {
        public override void Configure(EntityTypeBuilder<UserPostLike> builder)
        {
            base.Configure(builder);

            builder.ToTable("user_post_likes");

            builder.HasIndex(x => new { x.UserId, x.PostId })
                   .IsUnique();

            builder.Property(x => x.UserId)
                .HasColumnName("user_id");

            builder.Property(x => x.PostId)
                .HasColumnName("post_id");

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Post)
                .WithMany(x => x.Likes)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}