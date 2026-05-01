using System;
using System.Threading.Tasks;
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

        public async Task<string> GetCodeByIdAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var parsedId))
                return string.Empty; 

            var code = await _dbSet
                .AsNoTracking() 
                .Where(u => u.Id == parsedId)
                .Select(u => u.VerificationCode) 
                .FirstOrDefaultAsync();

            return code ?? string.Empty;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByRefreshAsync(string refresh)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.RefreshToken == refresh);
        }
    }
}