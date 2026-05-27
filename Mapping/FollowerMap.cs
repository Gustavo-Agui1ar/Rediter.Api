//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using Rediter.Api.Models.Chat;

//namespace Rediter.Api.Data.Mappings
//{
//    public class MessageMap : EntityMap<Message>
//    {
//        public override void Configure(EntityTypeBuilder<Message> builder)
//        {
//            base.Configure(builder);

//            builder.ToTable("messages");

//            builder.Property(x => x.ChatId)
//                .HasColumnName("chat_id");

//            builder.Property(x => x.SenderId)
//                .HasColumnName("sender_id");

//            builder.Property(x => x.Content)
//                .HasColumnName("content")
//                .HasMaxLength(4000)
//                .IsRequired();

//            builder.Property(x => x.CreatedAt)
//                .HasColumnName("created_at");

//            builder.Property(x => x.EditedAt)
//                .HasColumnName("edited_at");

//            builder.Property(x => x.ReadAt)
//                .HasColumnName("read_at");

//            builder.Property(x => x.DeletedAt)
//                .HasColumnName("deleted_at");

//            builder.HasIndex(x => x.ChatId);

//            builder.HasIndex(x => x.CreatedAt);

//            builder.HasOne(x => x.Chat)
//                .WithMany(x => x.Messages)
//                .HasForeignKey(x => x.ChatId)
//                .OnDelete(DeleteBehavior.Cascade);

//            builder.HasOne(x => x.Sender)
//                .WithMany(x => x.MessagesSent)
//                .HasForeignKey(x => x.SenderId)
//                .OnDelete(DeleteBehavior.Restrict);
//        }
//    }
//}