using Microsoft.EntityFrameworkCore;
using Rediter.Api.Data;
using Rediter.Api.DTOs;
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

        public async Task<User?> GetByEmail(string email)
        {
            string sql = "SELECT * FROM Users WHERE Email = {0}";

            return await _dbSet
                .FromSqlRaw(sql, email)
                .FirstOrDefaultAsync();
        }
        public async Task<User?> GetUserByRefresh(string refresh)
        {
            string sql = "SELECT * FROM Users WHERE refresh_token = {0}";

            return await _dbSet
                .FromSqlRaw(sql, refresh)
                .FirstOrDefaultAsync();
        }
    }
}
