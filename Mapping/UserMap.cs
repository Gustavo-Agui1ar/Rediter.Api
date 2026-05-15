using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Models;

namespace Rediter.Api.Mappings
{
    public class UserMap : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("ID");
            builder.Property(x => x.Name).HasColumnName("NAME").IsRequired().HasMaxLength(150);
            builder.Property(x => x.Email).HasColumnName("EMAIL").IsRequired().HasMaxLength(255);
            builder.Property(x => x.Password).HasColumnName("PASSWORD");
            builder.Property(x => x.CreatedAt).HasColumnName("CREATED_AT").IsRequired();
            builder.Property(x => x.IsVerified).HasColumnName("IS_VERIFIED").HasDefaultValue(false);
            builder.Property(x => x.VerificationCode).HasColumnName("VERIFICATION_CODE");
            builder.Property(x => x.RefreshToken).HasColumnName("REFRESH_TOKEN");
            builder.Property(x => x.RefreshTokenExpiration).HasColumnName("REFRESH_TOKEN_EXPIRATION");
            builder.Property(x => x.Description).HasColumnName("DESCRIPTION").HasMaxLength(500);

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
                .WithOne(x => x.Following)
                .HasForeignKey(x => x.FollowingId);

            builder.HasMany(x => x.Following)
                .WithOne(x => x.Follower)
                .HasForeignKey(x => x.FollowerId);

            builder.HasMany(x => x.BlockedUsers)
                .WithOne(x => x.Blocker)
                .HasForeignKey(x => x.BlockerId);

            builder.HasMany(x => x.BlockedBy)
                .WithOne(x => x.Blocked)
                .HasForeignKey(x => x.BlockedId);

            builder.Property<string>("NameLower")
                   .HasComputedColumnSql("LOWER(\"NAME\")"); 

            builder.HasIndex("NameLower")
                   .HasDatabaseName("idx_user_name_lower");
        }
    }
}