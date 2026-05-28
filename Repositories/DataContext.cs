using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Rediter.Api.Models;
using Rediter.Api.Models.Posts;
using Rediter.Api.Models.Users;
using System.Reflection;

namespace Rediter.Api.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (!string.IsNullOrEmpty(tableName))
                entityType.SetTableName(tableName.ToUpper());

            foreach (var property in entityType.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (!string.IsNullOrEmpty(columnName))
                    property.SetColumnName(columnName.ToUpper());

                if (property.ClrType == typeof(Guid))
                {
                    property.SetColumnType("RAW(16)");
                    property.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd;
                    property.SetValueGeneratorFactory((_, __) => new GuidValueGenerator());
                }

                if (property.ClrType == typeof(bool))
                {
                    property.SetColumnType("NUMBER(1)");
                }
            }
        }
    }
}