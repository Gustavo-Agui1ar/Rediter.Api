using System;
using System.Threading.Tasks;
using Rediter.Api.Models;
using Rediter.Api.Repositories;
using Rediter.Api.Services.UtilitariesServices;

namespace Rediter.Api.Services
{
    public class UserFollowerService : BaseService<UserFollower>
    {
        private readonly UserFollowerRepository _userFollowerRepository;

        public UserFollowerService(UserFollowerRepository userFollowerRepository) : base(userFollowerRepository)
        {
            _userFollowerRepository = userFollowerRepository;
        }

        public async Task<UserFollower?> GetRelationAsync(Guid followerId, Guid followingId)
        {
            return await _userFollowerRepository.GetRelationAsync(followerId, followingId);
        }
    }
}