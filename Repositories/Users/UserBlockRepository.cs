using Rediter.Api.Data;
using Rediter.Api.Models.Users;

namespace Rediter.Api.Repositories.Users
{
    public class UserBlockRepository : EntityRepository<UserBlock>
    {
        public UserBlockRepository(DataContext data) : base(data)
        {

        }
    }
}
