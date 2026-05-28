using Rediter.Api.Models.Posts;
using Rediter.Api.Repositories.Posts;
using Rediter.Api.Services.UtilitariesServices;

namespace Rediter.Api.Services.Posts
{
    public class PostImageService : BaseService<PostImage>
    {
        private readonly PostImageRepository _postImageRepository;

        public PostImageService(PostImageRepository postImageRepository) : base(postImageRepository)
        {
            _postImageRepository = postImageRepository;
        }
    }
}
