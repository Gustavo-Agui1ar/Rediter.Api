using Microsoft.EntityFrameworkCore;
using Rediter.Api.Data; 

namespace Rediter.Api.Repositories
{
    public class BaseRepository<T> where T : class
    {
        protected readonly DataContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(DataContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<bool> Insert(T entity)
        {
            await _dbSet.AddAsync(entity);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public virtual async Task<bool> Update(T entity)
        {
            _dbSet.Update(entity);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public virtual async Task<bool> Delete(T entity)
        {
            _dbSet.Remove(entity);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public virtual async Task<T?> GetById(object id)
        {
            return await _dbSet.FindAsync(id);
        }
    }
}