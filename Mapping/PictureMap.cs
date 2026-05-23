using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Data.Mappings;
using Rediter.Api.Models;

namespace Rediter.Api.Mappings;

public class PictureMap : EntityMap<Picture>
{
    public override void Configure(EntityTypeBuilder<Picture> builder)
    {
        base.Configure(builder);

        builder.ToTable("pictures");

        builder.Property(x => x.FileName)
            .HasColumnName("file_name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.StoragePath)
            .HasColumnName("storage_path")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.MimeType)
            .HasColumnName("mime_type")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Size)
            .HasColumnName("file_size_bytes")
            .IsRequired();

        builder.Ignore(x => x.FullPath);
        builder.Ignore(x => x.File);
    }
}