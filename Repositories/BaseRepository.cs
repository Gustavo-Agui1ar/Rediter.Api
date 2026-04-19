using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage; // Necessário para IDbContextTransaction
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

        #region Métodos de Transação (Escopo)

        /// <summary>
        /// Inicia uma nova transação assíncrona.
        /// </summary>
        public async Task<IDbContextTransaction> BeginTransaction()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// Confirma as alterações no banco de dados.
        /// </summary>
        public async Task CommitTransaction(IDbContextTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            await transaction.CommitAsync();
        }

        /// <summary>
        /// Desfaz as alterações caso algo dê errado.
        /// </summary>
        public async Task RollbackTransaction(IDbContextTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            await transaction.RollbackAsync();
        }

        #endregion

        /// <summary>
        /// Insere no banco a classe do repositorio 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<bool> Insert(T entity)
        {
            await _dbSet.AddAsync(entity);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        /// <summary>
        /// Atualiza a classe do repositorio
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<bool> Update(T entity)
        {
            _dbSet.Update(entity);

            var rows = await _context.SaveChangesAsync();

            return rows > 0;
        }
        /// <summary>
        /// Deleta do banco a classe
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<bool> Delete(T entity)
        {
            _dbSet.Remove(entity);

            var rows = await _context.SaveChangesAsync();

            return rows > 0;
        }

        /// <summary>
        /// Procura classe por uuid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<T?> GetByUuid(Guid uuid)
        {
            return await _dbSet.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "id") == uuid);
        }
        /// <summary>
        /// procura classe por id inteiro
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<T?> GetById(int id)
        {
            return await _dbSet.FindAsync(id);
        }
    }
}