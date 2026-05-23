using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Models;

namespace Rediter.Api.Data.Mappings
{
    public class UserBlockMap : EntityMap<UserBlock>
    {
        public override void Configure(EntityTypeBuilder<UserBlock> builder)
        {
            base.Configure(builder);

            builder.ToTable("user_blocks");

            builder.HasIndex(x => new { x.BlockerId, x.BlockedId })
                   .IsUnique();

            builder.Property(x => x.BlockerId)
                .HasColumnName("blocker_id");

            builder.Property(x => x.BlockedId)
                .HasColumnName("blocked_id");

            builder.HasOne(x => x.Blocker)
                .WithMany() 
                .HasForeignKey(x => x.BlockerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Blocked)
                .WithMany() 
                .HasForeignKey(x => x.BlockedId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}