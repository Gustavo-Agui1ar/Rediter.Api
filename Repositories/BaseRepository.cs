using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Rediter.Api.Data;
using Rediter.Api.Interfaces;

namespace Rediter.Api.Repositories
{
    public class BaseRepository<T> where T : class, IEntity
    {
        protected readonly DataContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(DataContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        /// <summary>
        /// Insere no banco a classe do repositorio 
        /// </summary>
        public virtual void Insert(T entity)
        {
            _dbSet.Add(entity);
        }

        /// <summary>
        /// Atualiza a classe do repositorio
        /// </summary>
        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        /// <summary>
        /// Deleta do banco a classe
        /// </summary>
        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Procura classe por uuid (com opção de AsNoTracking para performance)
        /// </summary>
        public virtual async Task<T?> GetByUuid(Guid uuid, bool trackChanges = true)
        {
            var query = trackChanges ? _dbSet : _dbSet.AsNoTracking();
            return await query.FirstOrDefaultAsync(e => e.Id == uuid);
        }

        /// <summary>
        /// procura classe por id inteiro (com opção de AsNoTracking)
        /// </summary>
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