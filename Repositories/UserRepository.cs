using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Rediter.Api.Data;
using Rediter.Api.DTOs;
using Rediter.Api.Models;

namespace Rediter.Api.Repositories
{
    public class UserRepository : BaseRepository<User>
    {
        public UserRepository(DataContext data) : base(data)
        {
        }

        public async Task<string> GetCodeByIdAsync(Guid userId)
        {
            var code = await _dbSet
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.VerificationCode)
                .FirstOrDefaultAsync();

            return code ?? string.Empty;
        }

        public async Task<User?> GetByEmailAsync(string email, bool trackChanges = true)
        {
            var query = trackChanges ? _dbSet : _dbSet.AsNoTracking();
            return await query.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByRefreshAsync(string refresh, bool trackChanges = true)
        {
            var query = trackChanges ? _dbSet : _dbSet.AsNoTracking();
            return await query.FirstOrDefaultAsync(u => u.RefreshToken == refresh);
        }

        public async Task<IList<UserFeedInfoDTO>> SearchUsers(
            string query,
            DateTime? lastCreatedAt,
            Guid? lastId,
            int pageSize,
            Guid currentUserId, 
            CancellationToken cancellationToken = default)
        {
            ValidatePaginationState(lastCreatedAt, lastId);

            IQueryable<User> usersQuery = _dbSet.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query))
            {
                usersQuery = usersQuery.Where(u => u.Name.Contains(query));
            }

            return await ApplyKeysetPagination(usersQuery, lastCreatedAt, lastId)
                .OrderByDescending(u => u.CreatedAt)
                .ThenByDescending(u => u.Id)
                .Select(MapToUserFeedInfoDTO(currentUserId))
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsFollowingAsync(Guid currentUserId, Guid targetUserId)
        {
            if (currentUserId == targetUserId)
                return false;

            return await _context.Set<UserFollower>()
                .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == targetUserId);
        }

        private static void ValidatePaginationState(DateTime? lastCreatedAt, Guid? lastId)
        {
            if (lastCreatedAt.HasValue != lastId.HasValue)
            {
                throw new ArgumentException("Para a paginação, 'lastCreatedAt' e 'lastId' devem ser fornecidos juntos ou ambos nulos.");
            }
        }

        private static Expression<Func<User, UserFeedInfoDTO>> MapToUserFeedInfoDTO(Guid currentUserId)
        {
            return u => new UserFeedInfoDTO
            {
                UserID = u.Id.ToString(),
                UserName = u.Name,
                ProfileImageName = u.ProfilePicture != null ? u.ProfilePicture.FileName : null,
                CreatedAt = u.CreatedAt,
                Description = u.Description,
                OwnProfile = u.Id == currentUserId,
                IsFollowing = currentUserId != Guid.Empty && u.Followers.Any(f => f.FollowerId == currentUserId)
            };
        }
    }
}