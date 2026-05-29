using Microsoft.EntityFrameworkCore;
using Rediter.Api.Data;
using Rediter.Api.DTOs.Users;
using Rediter.Api.Models.Users;
using Rediter.Api.Models.Chats;
using System.Linq.Expressions;

namespace Rediter.Api.Repositories.Users
{
    public class UserRepository : EntityRepository<User>
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

            string termLower = query.ToLower();

            if (!string.IsNullOrWhiteSpace(query))
                usersQuery = usersQuery.Where(u => EF.Property<string>(u, "NameLower").Contains(termLower));

            if (currentUserId != Guid.Empty)
            {   
                usersQuery = usersQuery.Where(u =>
                    !u.BlockedBy.Any(b => b.BlockerId == currentUserId) &&
                    !u.BlockedUsers.Any(b => b.BlockedId == currentUserId)
                 );
            }

            return await ApplyKeysetPagination(usersQuery, lastCreatedAt, lastId)
                .OrderByDescending(u => u.CreatedAt)
                .ThenByDescending(u => u.Id)
                .Select(MapToUserFeedInfoDTO(currentUserId))
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<UserFeedInfoDTO>> GetBlockedUsers(DateTime? lastCreatedAt, Guid? lastId, int pageSize, Guid currentUserID)
        {
            ValidatePaginationState(lastCreatedAt, lastId);

            var blockedUsersQuery = _dbSet.AsNoTracking()
                .Where(u => u.BlockedBy.Any(b => b.BlockerId == currentUserID));

            return await ApplyKeysetPagination(blockedUsersQuery, lastCreatedAt, lastId)
                .OrderByDescending(u => u.CreatedAt)
                .ThenByDescending(u => u.Id)
                .Select(MapToUserFeedInfoDTO(currentUserID)) 
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> IsFollowingAsync(Guid currentUserId, Guid targetUserId)
        {
            if (currentUserId == targetUserId)
                return false;

            return await _context.Set<Follower>()
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

        public async Task<UserDTO> GetUserProfileAsync(Guid targetUserId, Guid currentUserId)
        {
            return await _context.Set<User>()
                .Where(u => u.Id == targetUserId)
                .Select(u => new UserDTO
                {
                    UserID = u.Id,
                    Name = u.Name,
                    Email = u.Email,

                    ImageName = u.ProfilePicture != null ? u.ProfilePicture.FileName : null,
                    ImageCover = u.ProfileCover != null ? u.ProfileCover.FileName : null,
                    Description = u.Description,

                    IsFollowing = _context.Set<Follower>()
                        .Any(f => f.FollowerId == currentUserId && f.FollowingId == u.Id),

                    isBlocked = u.BlockedBy.Any(b => b.BlockerId == currentUserId),

                    Followers = u.FollowersCount,
                    Following = u.FollowingCount,
                    ChatId = _context.Set<Chat>()
                                .Where(c => !c.IsGroup &&
                                            c.Participants.Any(p => p.UserId == currentUserId) &&
                                            c.Participants.Any(p => p.UserId == u.Id))
                                .Select(c => (Guid?)c.Id) 
                                .FirstOrDefault()
                })
                .FirstOrDefaultAsync() ?? throw new Exception("User not found.");
        }
    }
}