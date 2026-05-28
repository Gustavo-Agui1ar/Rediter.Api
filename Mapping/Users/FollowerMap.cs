
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Models.Users;

namespace Rediter.Api.Data.Mappings.Users
{
    public class FollowerMap : EntityMap<Follower>
    {
        public override void Configure(EntityTypeBuilder<Follower> builder)
        {
            base.Configure(builder);

            builder.ToTable("user_followers");

            builder.HasIndex(x => new { x.FollowerId, x.FollowingId })
                   .IsUnique();

            builder.Property(x => x.FollowerId)
                .HasColumnName("follower_id");

            builder.Property(x => x.FollowingId)
                .HasColumnName("following_id");

            builder.HasOne(x => x.UserFollower)
                .WithMany(u => u.Following)
                .HasForeignKey(x => x.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.UserFollowing)
                .WithMany(u => u.Followers)
                .HasForeignKey(x => x.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}