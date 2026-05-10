using Rediter.Api.Data;

namespace Rediter.Api.Repositories
{
    public class UserFollowerRepository : BaseRepository<Models.UserFollower>
    {
        public UserFollowerRepository(DataContext data) : base(data)
        {

        }
    }
}
