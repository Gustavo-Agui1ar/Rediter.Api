using Microsoft.EntityFrameworkCore;
using Rediter.Api.Data;
using Rediter.Api.Models;

namespace Rediter.Api.Repositories
{
    public class PictureRepository : BaseRepository<Models.Picture>
    {
        public PictureRepository(DataContext data) : base(data)
        {

        }

        public async Task<List<Picture>> GetUnusedPictures()
        {
            string sql = @"
                SELECT p.*
                FROM Pictures p
                LEFT JOIN Users u ON u.profile_cover_id = p.Id
                LEFT JOIN Users u2 ON u2.profile_picture_id = p.Id
                LEFT JOIN post_images pi ON pi.picture_id = p.Id
                WHERE u.Id IS NULL 
                    AND u2.Id IS NULL
                    AND pi.picture_id IS NULL;
            ";

            return await _dbSet
                .FromSqlRaw(sql)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
