using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MiNET.Blocks;
using Rediter.Api.Data;
using Rediter.Api.DTOs;
using Rediter.Api.Models;

namespace Rediter.Api.Repositories
{
    public class PostRepository : BaseRepository<Post>
    {
        public PostRepository(DataContext data) : base(data)
        {
        }

        public async Task<IList<PostFeedDTO>> GetUserFeedAsync(
            Guid userId,
            DateTime? lastCreatedAt,
            Guid? lastId,
            int pageSize,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            ValidatePaginationState(lastCreatedAt, lastId);

            var query = _dbSet.AsNoTracking().Where(p => p.UserId == userId);

            return await ApplyKeysetPagination(query, lastCreatedAt, lastId)
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .Select(MapToPostFeedDTO(currentUserId))
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<string>> GetAllMidiaNames(Guid userId) 
        {
            return await _dbSet
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .SelectMany(p => p.PostImages)
                .Where(pi => pi.Picture != null)
                .OrderBy(pi => pi.CreatedAt)
                .Select(pi => pi.Picture!.FileName)
                .ToListAsync();
        }

        public async Task<IList<PostFeedDTO>> SearchPosts(
           string searchTerm,
           DateTime? lastCreatedAt,
           Guid? lastId,
           int pageSize,
           bool onlyWithMedia = false,
           Guid? parentPostID = null,
           bool fetchChildPosts = false,
           CancellationToken cancellationToken = default,
           Guid? currentUserId = null)
        {
            ValidatePaginationState(lastCreatedAt, lastId);

            var query = _dbSet.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var termLower = searchTerm.ToLower();
                query = query.Where(p =>
                    (p.User != null && EF.Property<string>(p.User, "NameLower").Contains(termLower)) ||
                    (p.Content != null && EF.Property<string>(p, "ContentLower").Contains(termLower))
                );
            }

            if (parentPostID.HasValue)
                query = query.Where(p => p.ParentPostId == parentPostID.Value);
            else if (!fetchChildPosts)
                query = query.Where(x => x.ParentPostId == null);

            if (onlyWithMedia)
                query = query.Where(p => p.PostImages.Any(pi => pi.Picture != null));

            Guid safeUserId = currentUserId ?? Guid.Empty;

            if (safeUserId != Guid.Empty)
            {
                query = query.Where(p =>
                    p.User != null &&
                    !p.User.BlockedBy.Any(b => b.BlockerId == safeUserId) &&
                    !p.User.BlockedUsers.Any(b => b.BlockedId == safeUserId)
                );
            }

            return await ApplyKeysetPagination(query, lastCreatedAt, lastId)
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .Select(MapToPostFeedDTO(safeUserId))
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<PostFeedDTO?> GetPostById(Guid postId, Guid currentUserId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(p => p.Id == postId)
                .Select(MapToPostFeedDTO(currentUserId))
                .FirstOrDefaultAsync();
        }

        public async Task AddLike(UserPostLike like)
        {
            await _context.Set<UserPostLike>().AddAsync(like);
        }

        public async Task IncrementLikesCount(Guid postId)
        {
            await _context.Posts
                .Where(p => p.Id == postId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.LikesCount, p => p.LikesCount + 1));
        }

        public async Task<IList<PostFeedDTO>> GetLikedPostsByUserAsync(
            Guid userId,
            DateTime? lastCreatedAt,
            Guid? lastId,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            ValidatePaginationState(lastCreatedAt, lastId);

            var query = _dbSet.AsNoTracking()
                .Where(p => p.Likes.Any(l => l.UserId == userId));

            return await ApplyKeysetPagination(query, lastCreatedAt, lastId)
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .Select(MapToPostFeedDTO(userId)) 
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<PostFeedDTO>> GetDiscoverPosts(
             Guid currentUserId,
             int? lastScore,
             DateTime? lastCreatedAt,
             Guid? lastId,
             int pageSize,
             CancellationToken cancellationToken = default)
        {
            ValidatePaginationState(lastCreatedAt, lastId);

            var query = _dbSet.AsNoTracking().Where(p => p.ParentPostId == null && p.UserId != currentUserId);

            if (lastScore.HasValue && lastCreatedAt.HasValue && lastId.HasValue)
            {
                query = query.Where(p =>
                    (p.LikesCount * 1 + p.CommentsCount * 2) < lastScore ||
                    ((p.LikesCount * 1 + p.CommentsCount * 2) == lastScore && p.CreatedAt < lastCreatedAt) ||
                    ((p.LikesCount * 1 + p.CommentsCount * 2) == lastScore && p.CreatedAt == lastCreatedAt && p.Id.CompareTo(lastId) < 0)
                );
            }

            return await query
                .OrderByDescending(p => (p.LikesCount * 1 + p.CommentsCount * 2))
                .ThenByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .Select(MapToPostFeedDTO(currentUserId))
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<PostFeedDTO>> GetFollowingPosts(
            Guid currentUserId,
            DateTime? lastCreatedAt,
            Guid? lastId,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            ValidatePaginationState(lastCreatedAt, lastId);

            var query = _dbSet.AsNoTracking()
                .Where(p => p.ParentPostId == null &&
                            p.User!.Followers.Any(f => f.FollowerId == currentUserId));

            if (lastCreatedAt.HasValue && lastId.HasValue)
            {
                query = query.Where(p =>
                    p.CreatedAt < lastCreatedAt ||
                    (p.CreatedAt == lastCreatedAt && p.Id.CompareTo(lastId) < 0)
                );
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .Select(MapToPostFeedDTO(currentUserId))
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        private static void ValidatePaginationState(DateTime? lastCreatedAt, Guid? lastId)
        {
            if (lastCreatedAt.HasValue != lastId.HasValue)
            {
                throw new ArgumentException("Para a paginação, 'lastCreatedAt' e 'lastId' devem ser fornecidos juntos ou ambos nulos.");
            }
        }

        private static Expression<Func<Post, PostFeedDTO>> MapToPostFeedDTO(Guid currentUserId)
        {
            return p => new PostFeedDTO
            {
                Id = p.Id.ToString(),
                UserName = p.User!.Name,
                ProfileImageName = p.User.ProfilePicture != null ? p.User.ProfilePicture.FileName : null,
                Text = p.Content,
                Location = p.LocationName,
                ImageUrls = p.PostImages
                    .Where(pi => pi.Picture != null)
                    .Select(pi => pi.Picture!.FileName)
                    .ToList(),
                Edited = p.CreatedAt != p.UpdatedAt,
                CreatedAt = p.CreatedAt,
                LikesCount = p.LikesCount,
                CommentsCount = p.CommentsCount, 
                LikedByCurrentUser = currentUserId != Guid.Empty && p.Likes.Any(l => l.UserId == currentUserId),
                PostUserID = p.UserId.ToString(),
                IsFollowing = currentUserId != Guid.Empty && p.User.Followers.Any(f => f.FollowerId == currentUserId),
                OwnPost = p.UserId == currentUserId,
                Score = (p.LikesCount * 1) + (p.CommentsCount * 2) /* + (p.Reposts.Count * 3)*/
            };
        }
    }
}