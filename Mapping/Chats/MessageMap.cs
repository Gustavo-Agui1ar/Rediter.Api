using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Data.Mappings;
using Rediter.Api.Models.Chats;

namespace Rediter.Api.Data;

public class MessageMap : EntityMap<Message>
{
    public override void Configure(EntityTypeBuilder<Message> builder)
    {
        base.Configure(builder);

        builder.ToTable("messages");

        builder.Property(x => x.ChatId).HasColumnName("CHAT_ID").IsRequired();
        builder.Property(x => x.SenderId).HasColumnName("SENDER_ID").IsRequired();

        builder.Property(x => x.Content)
               .HasColumnName("CONTENT")
               .IsRequired()
               .HasMaxLength(2000);

        builder.Property(x => x.IsDeleted).HasColumnName("IS_DELETED").HasDefaultValue(false);

        builder.HasOne(x => x.Chat)
               .WithMany(c => c.Messages)
               .HasForeignKey(x => x.ChatId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Sender)
               .WithMany()
               .HasForeignKey(x => x.SenderId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ChatId)
               .HasDatabaseName("idx_message_chat_id");
    }
}