using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Models;

namespace Rediter.Api.Mappings;

public class UserPostLikeMap
    : IEntityTypeConfiguration<UserPostLike>
{
    public void Configure(EntityTypeBuilder<UserPostLike> builder)
    {
        builder.ToTable("user_post_likes");

        builder.HasKey(x => new
        {
            x.UserId,
            x.PostId
        });

        builder.Property(x => x.UserId)
            .HasColumnName("user_id");

        builder.Property(x => x.PostId)
            .HasColumnName("post_id");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

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