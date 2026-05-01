using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration; // Necessário para o GuidValueGenerator
using Rediter.Api.Models;

namespace Rediter.Api.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entityTypes = typeof(Picture).Assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Namespace == "Rediter.Api.Models");

            foreach (var type in entityTypes)
            {
                modelBuilder.Entity(type);
            }

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.SetTableName(entityType.GetTableName()?.ToUpper());

                foreach (var property in entityType.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName().ToUpper());

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
}