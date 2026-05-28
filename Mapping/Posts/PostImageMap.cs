using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Data.Mappings;
using Rediter.Api.Models.Posts;

namespace Rediter.Api.Mapping.Posts;

public class PostImageMap : EntityMap<PostImage>
{
    public override void Configure(EntityTypeBuilder<PostImage> builder)
    {
        base.Configure(builder);

        builder.ToTable("post_images");

        builder.Property(x => x.PostId)
            .HasColumnName("post_id")
            .IsRequired();

        builder.Property(x => x.PictureId)
            .HasColumnName("picture_id")
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.HasOne(x => x.Post)
            .WithMany(p => p.PostImages) 
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Picture)
            .WithMany()
            .HasForeignKey(x => x.PictureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}