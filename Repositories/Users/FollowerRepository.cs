using Microsoft.EntityFrameworkCore;
using Rediter.Api.Data;
using Rediter.Api.Models.Users;

namespace Rediter.Api.Repositories.Users
{
    public class FollowerRepository : EntityRepository<Follower>
    {

        public FollowerRepository(DataContext data) : base(data)
        {
        }

        public async Task<Follower?> GetRelationAsync(Guid followerId, Guid followingId)
        {
            return await _context.Set<Follower>()
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        }
    }
}