using Microsoft.EntityFrameworkCore;
using MiNET.Blocks;
using Rediter.Api.Data;
using Rediter.Api.Models;
using System.Security.Cryptography.X509Certificates;

namespace Rediter.Api.Repositories
{
    public class PostRepository : BaseRepository<Post>
    {
        public PostRepository(DataContext data) : base(data)
        {

        }

        public async Task<IList<Post>> GetPostsByUser(
            string userId,
            DateTime? lastCreatedAt,
            string? lastId,
            int pageSize)
        {
            var parsedUserId = Guid.Parse(userId);
            var query = _dbSet.AsNoTracking().Where(p => p.UserId == parsedUserId);

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
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IList<string>> GetAllMidiaNames(string userId)
        {
            var parsedUserId = Guid.Parse(userId);

            return await _dbSet
                .AsNoTracking()
                .Where(p => p.UserId == parsedUserId)
                .SelectMany(p => p.PostImages)
                .Where(pi => pi.Picture != null)
                .OrderByDescending(pi => pi.CreatedAt)
                .Select(pi => pi.Picture!.FileName)
                .ToListAsync();
        }
    }
}
