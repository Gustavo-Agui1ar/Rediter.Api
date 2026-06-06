using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Data.Mappings;
using Rediter.Api.Models.Users;

namespace Rediter.Api.Mapping.Users;

public class UserMap : EntityMap<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable("users");

        builder.Property(x => x.Name).HasColumnName("NAME").IsRequired().HasMaxLength(150);
        builder.Property(x => x.Email).HasColumnName("EMAIL").IsRequired().HasMaxLength(255);
        builder.Property(x => x.Password).HasColumnName("PASSWORD");
        builder.Property(x => x.IsVerified).HasColumnName("IS_VERIFIED").HasDefaultValue(false);
        builder.Property(x => x.IsDeleted).HasColumnName("IS_DELETED").HasDefaultValue(false);
        builder.Property(x => x.VerificationCode).HasColumnName("VERIFICATION_CODE");
        builder.Property(x => x.VerificationCodeExpiration).HasColumnName("VERIFICATION_CODE_EXPIRATION");
        builder.Property(x => x.RefreshToken).HasColumnName("REFRESH_TOKEN");
        builder.Property(x => x.RefreshTokenExpiration).HasColumnName("REFRESH_TOKEN_EXPIRATION");
        builder.Property(x => x.Description).HasColumnName("DESCRIPTION").HasMaxLength(500);

        builder.Property(x => x.FollowersCount).HasColumnName("FOLLOWERS_COUNT").HasDefaultValue(0);
        builder.Property(x => x.FollowingCount).HasColumnName("FOLLOWING_COUNT").HasDefaultValue(0);

        builder.Property(x => x.DeviceToken).HasColumnName("DEVICE_TOKEN").HasMaxLength(255);

        builder.Property(x => x.ProfilePictureId).HasColumnName("PROFILE_PICTURE_ID");
        builder.HasOne(x => x.ProfilePicture)
            .WithMany()
            .HasForeignKey(x => x.ProfilePictureId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.ProfileCoverId).HasColumnName("PROFILE_COVER_ID");
        builder.HasOne(x => x.ProfileCover)
            .WithMany()
            .HasForeignKey(x => x.ProfileCoverId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Followers)
            .WithOne(x => x.UserFollowing)
            .HasForeignKey(x => x.FollowingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Following)
            .WithOne(x => x.UserFollower)
            .HasForeignKey(x => x.FollowerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.BlockedUsers)
            .WithOne(x => x.Blocker)
            .HasForeignKey(x => x.BlockerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.BlockedBy)
            .WithOne(x => x.Blocked)
            .HasForeignKey(x => x.BlockedId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Roles)
           .WithMany(r => r.Users)
           .UsingEntity<Dictionary<string, object>>(
               "USER_ROLES",
               j => j.HasOne<Role>().WithMany().HasForeignKey("ROLE_ID"),
               j => j.HasOne<User>().WithMany().HasForeignKey("USER_ID")
           );

        builder.Property<string>("NameLower")
               .HasComputedColumnSql("LOWER(\"NAME\")");

        builder.HasIndex("NameLower")
               .HasDatabaseName("idx_user_name_lower");
    }
}