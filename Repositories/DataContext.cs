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
            modelBuilder.Entity<User>().Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");

            base.OnModelCreating(modelBuilder);
        }
    }
}