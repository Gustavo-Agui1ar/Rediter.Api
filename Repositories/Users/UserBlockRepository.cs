using Microsoft.EntityFrameworkCore;
using Rediter.Api.Data;
using Rediter.Api.Models.Users;

namespace Rediter.Api.Repositories.Users
{
    public class UserBlockRepository : EntityRepository<UserBlock>
    {
        public UserBlockRepository(DataContext data) : base(data)
        {

        }

        public async Task<UserBlock?> GetBlockByUserIds(Guid blockerId, Guid blockedId)
        {
            return await _context.Set<UserBlock>().FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId);
        }
    }
}
