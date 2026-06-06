using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Rediter.Api.Data;
using Rediter.Api.Interfaces;

namespace Rediter.Api.Repositories
{
    public class EntityRepository<T> where T : class, IEntity
    {
        protected readonly DataContext _context;
        protected readonly DbSet<T> _dbSet;

        public EntityRepository(DataContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual void Insert(T entity)
        {
            _dbSet.Add(entity);
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public virtual async Task<T?> GetByUuid(Guid uuid, bool trackChanges = true)
        {
            var query = trackChanges ? _dbSet : _dbSet.AsNoTracking();
            return await query.FirstOrDefaultAsync(e => e.Id == uuid);
        }

        public virtual async Task<T?> GetById(int id, bool trackChanges = true)
        {
            if (trackChanges)
            {
                return await _dbSet.FindAsync(id);
            }

            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
        }

        protected IQueryable<T> ApplyKeysetPagination(IQueryable<T> query, DateTime? lastCreatedAt, Guid? lastId)
        {
            if (lastCreatedAt.HasValue && lastId.HasValue)
            {
                return query.Where(e =>
                    e.CreatedAt < lastCreatedAt.Value ||
                    (e.CreatedAt == lastCreatedAt.Value && e.Id.CompareTo(lastId.Value) < 0)
                );
            }
            return query;
        }

        public async Task SaveChanges()
        {
            int rows = await _context.SaveChangesAsync();

            if (rows <= 0)
            {
                throw new Exception("No changes were saved to the database.");
            }
        }
    }
}