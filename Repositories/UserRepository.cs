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

        public async Task<List<UserFeedInfoDTO>> SearchUsers(string query, DateTime? lastCreatedAt, string? lastId, int pageSize)
        {
            IQueryable<User> usersQuery = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                usersQuery = usersQuery.Where(u => u.Name.ToUpper().Contains(query.ToUpper()));
            }

            if (lastCreatedAt.HasValue && !string.IsNullOrWhiteSpace(lastId))
            {
                usersQuery = usersQuery.Where(u =>
                    u.CreatedAt < lastCreatedAt ||
                    (u.CreatedAt == lastCreatedAt && string.Compare(u.Id.ToString(), lastId) < 0));
            }

            var users = await usersQuery
                .OrderByDescending(u => u.CreatedAt)
                .ThenByDescending(u => u.Id)
                .Take(pageSize)
                .Select(u => new UserFeedInfoDTO
                {
                    UserID = u.Id.ToString(),
                    UserName = u.Name,
                    ProfileImageName = u.ProfilePicture != null ? u.ProfilePicture.FileName : null,
                    CreatedAt = u.CreatedAt,
                    Description = u.Description
                })
                .ToListAsync();

            return users;
        }
    }
}