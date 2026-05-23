using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Models;

namespace Rediter.Api.Data.Mappings
{
    public class NotificationMap : EntityMap<Notification>
    {
        public override void Configure(EntityTypeBuilder<Notification> builder)
        {
            base.Configure(builder);

            builder.ToTable("notifications");

            builder.Property(x => x.RecipientUserId)
                .HasColumnName("recipient_user_id")
                .IsRequired();

            builder.Property(x => x.SenderUserId)
                .HasColumnName("sender_user_id")
                .IsRequired();

            builder.Property(x => x.PostId)
                .HasColumnName("post_id");

            builder.Property(x => x.Type)
                .HasColumnName("type")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.IsRead)
                .HasColumnName("is_read")
                .HasDefaultValue(false);

            builder.HasIndex(x => x.RecipientUserId);

            builder.HasIndex(x => x.CreatedAt);

            builder.HasIndex(x => new
            {
                x.RecipientUserId,
                x.IsRead
            });

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.RecipientUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Post>()
                .WithMany()
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}