using Rediter.Api.Data;

namespace Rediter.Api.Repositories
{
    public class UserBlockRepository : BaseRepository<Models.UserBlock>
    {
        public UserBlockRepository(DataContext data) : base(data)
        {

        }
    }
}
