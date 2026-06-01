using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Data.Mappings;
using Rediter.Api.Models;
using Rediter.Api.Models.Users;

namespace Rediter.Api.Mappings;

public class RoleMap : EntityMap<Role>
{
    public override void Configure(EntityTypeBuilder<Role> builder)
    {
        base.Configure(builder);

        builder.ToTable("roles");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(255);
    }
}