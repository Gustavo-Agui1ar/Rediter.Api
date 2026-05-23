using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Models;

namespace Rediter.Api.Data.Mappings
{
    public class UserFollowerMap : EntityMap<UserFollower>
    {
        public override void Configure(EntityTypeBuilder<UserFollower> builder)
        {
            base.Configure(builder);

            builder.ToTable("user_followers");

            builder.HasIndex(x => new { x.FollowerId, x.FollowingId })
                   .IsUnique();

            builder.Property(x => x.FollowerId)
                .HasColumnName("follower_id");

            builder.Property(x => x.FollowingId)
                .HasColumnName("following_id");

            builder.HasOne(x => x.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(x => x.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(x => x.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}