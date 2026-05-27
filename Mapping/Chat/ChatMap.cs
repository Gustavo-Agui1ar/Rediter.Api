using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Data.Mappings;

namespace Rediter.Api.Mapping;

public class ChatMap : EntityMap<Models.Chat.Chat>
{
    public override void Configure(EntityTypeBuilder<Models.Chat.Chat> builder)
    {
        base.Configure(builder);

        builder.ToTable("chats");

        builder.Property(x => x.Title)
               .HasColumnName("TITLE")
               .HasMaxLength(150);

        builder.Property(x => x.IsGroup)
               .HasColumnName("IS_GROUP")
               .HasDefaultValue(false);
    }
}