using Microsoft.EntityFrameworkCore;
using Rediter.Api.Data;
using Rediter.Api.DTOs.Users;
using Rediter.Api.Models.Users;
using Rediter.Api.Models.Chats;
using System.Linq.Expressions;
using Rediter.Api.Models.Posts;

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

        public async Task<Role> GetRoleByNameAsync(string roleName)
        {
            var role = await _context.Set<Role>()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
                throw new Exception($"Role '{roleName}' not found.");
            return role;
        }

        public async Task<User?> GetByEmailAsync(string email, bool trackChanges = true)
        {
            var query = trackChanges ? _dbSet : _dbSet.AsNoTracking();

            return await query
                .Include(u => u.Roles) 
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            var user = await _context.FindAsync<User>(userId);
            if (user == null) return;

            var followingRelations = await _context.Set<Follower>()
                .Where(f => f.FollowerId == userId).ToListAsync();

            foreach (var relation in followingRelations)
            {
                var followedUser = await _context.FindAsync<User>(relation.FollowingId);
                if (followedUser != null && followedUser.FollowersCount > 0)
                    followedUser.FollowersCount--;
            }

            var followerRelations = await _context.Set<Follower>()
                .Where(f => f.FollowingId == userId).ToListAsync();

            foreach (var relation in followerRelations)
            {
                var followerUser = await _context.FindAsync<User>(relation.FollowerId);
                if (followerUser != null && followerUser.FollowingCount > 0)
                    followerUser.FollowingCount--;
            }

            _context.Set<Follower>().RemoveRange(followingRelations);
            _context.Set<Follower>().RemoveRange(followerRelations);

            var blocks = await _context.Set<UserBlock>()
                .Where(b => b.BlockerId == userId || b.BlockedId == userId).ToListAsync();
            _context.Set<UserBlock>().RemoveRange(blocks);

            user.IsDeleted = true;
            user.Name = "Usuário Excluído";
            user.Email = $"deleted_{Guid.NewGuid()}@deleted.com";
            user.ProfilePictureId = null;
            user.ProfileCoverId = null;
            user.DeviceToken = null;
            user.Password = null;
            user.Description = "";
            user.FollowersCount = 0;
            user.FollowingCount = 0;

            await _context.SaveChangesAsync();
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

            usersQuery = usersQuery.Where(u => u.Id != currentUserId && !u.IsDeleted);

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

        public async Task<Role?> GetRoleDefault()
        {

            var defaultRole = await _context.Set<Role>()
                              .FirstOrDefaultAsync(r => r.Name == "Default");
            return defaultRole;
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

        public async Task<DashboardMetricsDTO> GetMetricsAsync()
        {
            var today = DateTime.UtcNow.Date;

            var totalUsers = await _context.Set<User>().IgnoreQueryFilters().CountAsync();
            var totalPosts = await _context.Set<Post>().CountAsync();

            var postsToday = await _context.Set<Post>()
                .Where(p => p.CreatedAt >= today)
                .CountAsync();

            return new DashboardMetricsDTO
            {
                TotalUsers = totalUsers,
                TotalPosts = totalPosts,
                TotalPostsToday = postsToday,
                OnlineUsersNow = 0 
            };
        }
    }
}