using Rediter.Api.Data;
using Rediter.Api.Models;

namespace Rediter.Api.Repositories
{
    public class UserRepository : BaseRepository<Models.User>
    {
        public UserRepository(DataContext data) : base(data)
        {
        }
        public string GetCodeById(string userId)
        {
            User? user = _dbSet.Find(Guid.Parse(userId));
            return user?.VerificationCode ?? string.Empty;
        }
    }
}
