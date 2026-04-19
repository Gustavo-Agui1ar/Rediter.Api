using Rediter.Api.Data;

namespace Rediter.Api.Repositories
{
    public class PostRepository : BaseRepository<Models.Post>
    {
        public PostRepository(DataContext data) : base(data)
        {

        }
    }
}
