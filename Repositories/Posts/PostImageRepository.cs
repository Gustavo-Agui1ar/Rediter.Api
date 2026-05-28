using Rediter.Api.Data;
using Rediter.Api.Models.Posts;

namespace Rediter.Api.Repositories.Posts
{
    public class PostImageRepository : EntityRepository<PostImage>
    {
        public PostImageRepository(DataContext data) : base(data)
        {
           
        }

    }
}
