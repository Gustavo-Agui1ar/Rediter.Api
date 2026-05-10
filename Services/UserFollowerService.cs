using Rediter.Api.Models;
using Rediter.Api.Repositories;
using Rediter.Api.Services.UtilitariesServices;

namespace Rediter.Api.Services
{
    public class UserFollowerService : BaseService<UserFollower>
    {
        private readonly UserFollowerRepository userFollowerRepository;

        public UserFollowerService(UserFollowerRepository userFollowerRepository) : base(userFollowerRepository)
        {
            this.userFollowerRepository = userFollowerRepository;
        }
    }
}
