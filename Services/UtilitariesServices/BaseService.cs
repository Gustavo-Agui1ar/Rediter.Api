using Rediter.Api.Repositories;

namespace Rediter.Api.Services.UtilitariesServices
{
    public class BaseService<T> : IService<T> where T : class
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

        public virtual async Task<bool> InsertAsync(T entity)
        {
            return await _repository.Insert(entity);
        }

        public virtual async Task<bool> UpdateAsync(T entity)
        {
            return await _repository.Update(entity);
        }

        public virtual async Task<bool> DeleteByIdAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;

            return await _repository.Delete(entity);
        }

        public virtual async Task<bool> DeleteByGuidAsync(Guid guid)
        {
            var entity = await GetByGuidAsync(guid);
            if (entity == null) return false;

            return await _repository.Delete(entity);
        }
    }
}