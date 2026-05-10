using Microsoft.EntityFrameworkCore;
using MiNET.Blocks;
using Rediter.Api.Data;
using Rediter.Api.DTOs;
using Rediter.Api.Models;
using System.Security.Cryptography.X509Certificates;

namespace Rediter.Api.Repositories
{
    public class PostRepository : BaseRepository<Post>
    {
        private readonly UserPostLikeRepository _likeRepository;
        public PostRepository(DataContext data, UserPostLikeRepository likeRepository) : base(data)
        {
            _likeRepository = likeRepository;
        }

        public async Task<IList<PostFeedDTO>> GetUserFeedAsync( 
            string userId,
            DateTime? lastCreatedAt,
            string? lastId,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var parsedUserId = Guid.Parse(userId);
            var query = _dbSet.AsNoTracking().Where(p => p.UserId == parsedUserId);

            if (lastCreatedAt.HasValue != !string.IsNullOrEmpty(lastId))
            {
                throw new ArgumentException("Para a paginação, 'lastCreatedAt' e 'lastId' devem ser fornecidos juntos ou ambos nulos.");
            }

            if (lastCreatedAt.HasValue && !string.IsNullOrEmpty(lastId))
            {
                var parsedLastId = Guid.Parse(lastId);

                query = query.Where(p =>
                    p.CreatedAt < lastCreatedAt.Value ||
                    (p.CreatedAt == lastCreatedAt.Value && p.Id.CompareTo(parsedLastId) < 0)
                );
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .Select(p => new PostFeedDTO
                {
                    Id = p.Id.ToString(),
                    UserName = p.User!.Name,
                    ImageProfileUrl = p.User.ProfilePicture != null ? p.User.ProfilePicture.FileName : null,
                    Text = p.Content,
                    Location = p.LocationName,
                    ImageUrls = p.PostImages
                        .Select(pi => pi.Picture != null ? pi.Picture.FileName : null)
                        .Where(fileName => fileName != null)
                        .Select(fileName => fileName!) 
                        .ToList(),
                    Edited = p.CreatedAt != p.UpdatedAt,
                    CreatedAt = p.CreatedAt,
                    LikesCount = p.LikesCount
                })
                .Take(pageSize)
                .ToListAsync(cancellationToken); 
        }

        public async Task<IList<string>> GetAllMidiaNames(string userId)
        {
            var parsedUserId = Guid.Parse(userId);

            return await _dbSet
                .AsNoTracking()
                .Where(p => p.UserId == parsedUserId)
                .SelectMany(p => p.PostImages)
                .Where(pi => pi.Picture != null)
                .OrderBy(pi => pi.CreatedAt)
                .Select(pi => pi.Picture!.FileName)
                .ToListAsync();
        }

        public async Task<IList<PostFeedDTO>> SearchFeedAsync(
             string searchTerm,
             DateTime? lastCreatedAt,
             string? lastId,
             int pageSize,
             bool onlyWithMedia = false, 
             CancellationToken cancellationToken = default,
             string userId = "")
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var termLower = searchTerm.ToLower();
                query = query.Where(p =>
                    (p.User != null && p.User.Name.ToLower().Contains(termLower)) ||
                    (p.Content != null && p.Content.ToLower().Contains(termLower))
                );
            }

            if (onlyWithMedia)
            {
                query = query.Where(p => p.PostImages.Any());
            }

            if (lastCreatedAt.HasValue != !string.IsNullOrEmpty(lastId))
            {
                throw new ArgumentException("Para a paginação, 'lastCreatedAt' e 'lastId' devem ser fornecidos juntos ou ambos nulos.");
            }

            if (lastCreatedAt.HasValue && !string.IsNullOrEmpty(lastId))
            {
                var parsedLastId = Guid.Parse(lastId);

                query = query.Where(p =>
                    p.CreatedAt < lastCreatedAt.Value ||
                    (p.CreatedAt == lastCreatedAt.Value && p.Id.CompareTo(parsedLastId) < 0)
                );
            }

            Guid userUuid = new Guid(userId);

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .Select(p => new PostFeedDTO
                {
                    Id = p.Id.ToString(),
                    UserName = p.User!.Name,
                    ImageProfileUrl = p.User.ProfilePicture != null ? p.User.ProfilePicture.FileName : null,
                    Text = p.Content,
                    Location = p.LocationName,
                    ImageUrls = p.PostImages
                        .Select(pi => pi.Picture != null ? pi.Picture.FileName : null)
                        .Where(fileName => fileName != null)
                        .Select(fileName => fileName!)
                        .ToList(),
                    Edited = p.CreatedAt != p.UpdatedAt,
                    CreatedAt = p.CreatedAt,
                    LikesCount = p.LikesCount,
                    LikedByCurrentUser = p.Likes.Any(l => l.UserId == userUuid)
                })
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
        public async Task AddLike(UserPostLike like)
        {
            await _likeRepository.Insert(like);
        }

        public async Task IncrementLikesCount(Guid postId)
        {
            await _context.Posts
                .Where(p => p.Id == postId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.LikesCount, p => p.LikesCount + 1));
        }
    }
}
