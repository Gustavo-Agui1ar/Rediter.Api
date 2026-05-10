using Microsoft.EntityFrameworkCore;
using Rediter.Api.Data;

namespace Rediter.Api.Repositories
{
    public class UserPostLikeRepository : BaseRepository<Models.UserPostLike>
    {
        public UserPostLikeRepository(DataContext data) : base(data)
        {

        }
    }
}
