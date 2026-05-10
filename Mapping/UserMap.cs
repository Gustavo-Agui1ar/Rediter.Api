using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Models;

namespace Rediter.Api.Mappings;

public class UserMap : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(150);
        builder.Property(x => x.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
        builder.Property(x => x.Password).HasColumnName("password");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.IsVerified).HasColumnName("is_verified").HasDefaultValue(false);
        builder.Property(x => x.VerificationCode).HasColumnName("verification_code");
        builder.Property(x => x.RefreshToken).HasColumnName("refresh_token");
        builder.Property(x => x.RefreshTokenExpiration).HasColumnName("refresh_token_expiration");
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);

        builder.Property(x => x.ProfilePictureId).HasColumnName("profile_picture_id");
        builder.HasOne(x => x.ProfilePicture)
            .WithMany()
            .HasForeignKey(x => x.ProfilePictureId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.ProfileCoverId).HasColumnName("profile_cover_id");
        builder.HasOne(x => x.ProfileCover)
            .WithMany()
            .HasForeignKey(x => x.ProfileCoverId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Followers)
            .WithOne(x => x.Following)
            .HasForeignKey(x => x.FollowingId);

        builder.HasMany(x => x.Following)
            .WithOne(x => x.Follower)
            .HasForeignKey(x => x.FollowerId);
    }
}