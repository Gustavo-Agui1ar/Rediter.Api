using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Models.Chats;

namespace Rediter.Api.Data.Mappings;

public class ChatParticipantMap : EntityMap<ChatParticipant>
{
    public override void Configure(EntityTypeBuilder<ChatParticipant> builder)
    {
        base.Configure(builder);

        builder.ToTable("chat_participants");

        builder.Property(x => x.ChatId).HasColumnName("CHAT_ID").IsRequired();
        builder.Property(x => x.UserId).HasColumnName("USER_ID").IsRequired();
        builder.Property(x => x.IsMuted).HasColumnName("IS_MUTED").HasDefaultValue(false);

        builder.Property(x => x.LastReadAt)
               .HasColumnName("LAST_READ_AT")
               .IsRequired(false);

        builder.HasOne(x => x.Chat)
               .WithMany(c => c.Participants)
               .HasForeignKey(x => x.ChatId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
               .WithMany(u => u.Chats)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.ChatId, x.UserId })
               .IsUnique()
               .HasDatabaseName("idx_chat_participant_unique");
    }
}