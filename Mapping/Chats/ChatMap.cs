using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Data.Mappings;
using Rediter.Api.Models.Chats;

namespace Rediter.Api.Mapping;
public class ChatMap : EntityMap<Chat>
{
    public override void Configure(EntityTypeBuilder<Chat> builder)
    {
        base.Configure(builder);

        builder.ToTable("chats");

        builder.Property(x => x.Title)
               .HasColumnName("TITLE")
               .HasMaxLength(150);

        builder.Property(x => x.IsGroup)
               .HasColumnName("IS_GROUP")
               .HasDefaultValue(false);

        builder.Property(x => x.UpdateAt)
               .HasColumnName("UPDATE_AT")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasMany(c => c.Participants) 
           .WithOne(p => p.Chat)
           .HasForeignKey(p => p.ChatId)
           .OnDelete(DeleteBehavior.Cascade);
    }
}