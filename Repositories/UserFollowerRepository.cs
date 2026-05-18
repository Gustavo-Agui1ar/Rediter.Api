using Microsoft.EntityFrameworkCore;
using Rediter.Api.Data;
using Rediter.Api.Models;

namespace Rediter.Api.Repositories
{
    public class UserFollowerRepository : BaseRepository<UserFollower>
    {

        public UserFollowerRepository(DataContext data) : base(data)
        {
        }

        public async Task<UserFollower?> GetRelationAsync(Guid followerId, Guid followingId)
        {
            return await _context.Set<UserFollower>()
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        }
    }
}