using Supabase.Postgrest.Models;

namespace Rediter.Api.Repositories
{
    public class BaseRepository<T> where T : BaseModel, new()
    {
        protected readonly Supabase.Client _supabase;
        protected readonly Session _session;
        public BaseRepository(Supabase.Client supabase, Session session)
        {
            _supabase = supabase;
            _session = session;
        }

        public virtual async Task<bool> Insert(T entity)
        {
            var result = await _supabase.From<T>().Insert(entity);
            return result.Model != null;
        }
        public virtual void Update(T entity)
        {
            _supabase.From<T>().Update(entity);
        }

        public virtual async Task<bool> Delete(T entity)
        {
            var result = await _supabase.From<T>().Delete(entity);
            return result.Model != null;
        }
    }
}
