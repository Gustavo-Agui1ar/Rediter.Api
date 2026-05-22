namespace Rediter.Api.Interfaces
{
    public interface IService<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetByGuidAsync(Guid guid);

        void Insert(T entity);
        void Update(T entity);
        void Delete(T entity);

        Task<bool> DeleteByIdAsync(int id);
        Task<bool> DeleteByGuidAsync(Guid guid);
    }
}