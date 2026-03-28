using Rediter.Api.Data;

namespace Rediter.Api.Repositories
{
    public class UserRepository : BaseRepository<Models.User>
    {
        public UserRepository(DataContext data) : base(data)
        {

        }
    }
}
