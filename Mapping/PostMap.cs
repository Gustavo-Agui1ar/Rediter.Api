using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Models;

namespace Rediter.Api.Data.Mappings
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.ToTable("Posts");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Content)
                .HasMaxLength(2000); 

            builder.Property(p => p.LocationName)
                .HasMaxLength(255);

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); 
            builder.Property(p => p.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.ParentPost)
                .WithMany(p => p.Replies)
                .HasForeignKey(p => p.ParentPostId)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.HasMany(p => p.PostImages)
                .WithOne() 
                .HasForeignKey("PostId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Likes)
                .WithOne()
                .HasForeignKey("PostId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}