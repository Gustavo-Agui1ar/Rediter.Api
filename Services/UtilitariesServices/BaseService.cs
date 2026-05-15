using Rediter.Api.Models;
using Rediter.Api.Repositories;

namespace Rediter.Api.Services.UtilitariesServices
{
    public class BaseService<T> : IService<T> where T : class, IEntity
    {
        protected readonly BaseRepository<T> _repository;

        public BaseService(BaseRepository<T> repository)
        {
            _repository = repository;
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _repository.GetById(id);
        }

        public virtual async Task<T?> GetByGuidAsync(Guid guid)
        {
            return await _repository.GetByUuid(guid);
        }

        public virtual void Insert(T entity)
        {
            _repository.Insert(entity);
        }

        public virtual void Update(T entity)
        {
            _repository.Update(entity);
        }

        public virtual async Task<bool> DeleteByIdAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;

            _repository.Delete(entity);
            return true;
        }

        public virtual async Task<bool> DeleteByGuidAsync(Guid guid)
        {
            var entity = await GetByGuidAsync(guid);
            if (entity == null) return false;

            _repository.Delete(entity);
            return true;
        }

        public virtual void Delete(T entity)
        {
            _repository.Delete(entity);
        }

        public virtual async Task SaveChangesAsync()
        {
            await _repository.SaveChanges();
        }
    }
}