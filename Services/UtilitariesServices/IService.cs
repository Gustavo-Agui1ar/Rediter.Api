namespace Rediter.Api.Services.UtilitariesServices
{
    public interface IService<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<T?> GetByGuidAsync(Guid guid);

        Task<bool> InsertAsync(T entity);
        Task<bool> UpdateAsync(T entity);

        Task<bool> DeleteByIdAsync(int id);
        Task<bool> DeleteByGuidAsync(Guid guid);
        Task<bool> DeleteAsync(T entity);
    }
}