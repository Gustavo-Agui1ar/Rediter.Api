using Microsoft.EntityFrameworkCore;
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
                var idProperty = entityType.FindProperty("Id");

                if (idProperty != null && idProperty.ClrType == typeof(Guid))
                {
                    idProperty.SetDefaultValueSql("gen_random_uuid()");
                }
            }
        }
    }
}