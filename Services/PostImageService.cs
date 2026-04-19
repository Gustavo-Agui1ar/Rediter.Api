using Rediter.Api.Models;
using Rediter.Api.Repositories;
using Rediter.Api.Services.UtilitariesServices;

namespace Rediter.Api.Services
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
