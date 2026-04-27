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
            string sql = @"
                SELECT p.*
                FROM Posts p
                WHERE p.user_id = @userId
                    AND (
                        @lastCreatedAt::timestamp IS NULL
                        OR p.created_at < @lastCreatedAt::timestamp
                        OR (p.created_at = @lastCreatedAt::timestamp AND p.id < @lastId::uuid)
                        )
                ORDER BY p.created_at DESC, p.id DESC
                LIMIT @pageSize;";

            var parameters = new[]
            {
                new Npgsql.NpgsqlParameter("userId", Guid.Parse(userId)),
                new Npgsql.NpgsqlParameter("lastCreatedAt", (object?)lastCreatedAt ?? DBNull.Value),
                new Npgsql.NpgsqlParameter("lastId", lastId != null ? Guid.Parse(lastId) : DBNull.Value),
                new Npgsql.NpgsqlParameter("pageSize", pageSize)
            };

            return await _dbSet
                .FromSqlRaw(sql, parameters)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IList<string>> GetAllMidiaNames(string userId) {
            string sql = @"
                SELECT pic.file_name
                FROM Posts p
                JOIN post_images pi ON p.id = pi.post_id
                JOIN Pictures pic ON pi.picture_id = pic.id
                WHERE p.user_id = @userId
                ORDER BY pi.created_at DESC;";
            
            var parameters = new[]
            {
                new Npgsql.NpgsqlParameter("userId", Guid.Parse(userId))
            };

            return await _context.Database
                .SqlQueryRaw<string>(sql, parameters)
                .ToListAsync();
        }
    }
}
