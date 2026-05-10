using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Models;

namespace Rediter.Api.Data.Mappings;

public class UserFollowerMap : IEntityTypeConfiguration<UserFollower>
{
    public void Configure(EntityTypeBuilder<UserFollower> builder)
    {
        builder.ToTable("user_followers");

        builder.HasKey(x => new { x.FollowerId, x.FollowingId });

        builder.Property(x => x.FollowerId)
            .HasColumnName("follower");

        builder.Property(x => x.FollowingId)
            .HasColumnName("following");

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