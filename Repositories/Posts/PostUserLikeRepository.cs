using Microsoft.EntityFrameworkCore;
using Rediter.Api.Data;

namespace Rediter.Api.Repositories.Posts
{
    public class PostUserLikeRepository
        : EntityRepository<UserPostLike>
    {
        public PostUserLikeRepository(DataContext data)
            : base(data)
        {
        }

        public async Task<bool> Exists(
            Guid postId,
            Guid userId)
        {
            return await _dbSet
                .AnyAsync(x =>
                    x.PostId == postId &&
                    x.UserId == userId);
        }

        public async Task<UserPostLike?> GetByPostAndUser(
            Guid postId,
            Guid userId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x =>
                    x.PostId == postId &&
                    x.UserId == userId);
        }
    }
}