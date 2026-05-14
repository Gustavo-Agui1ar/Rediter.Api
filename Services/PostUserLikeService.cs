using Rediter.Api.Models;
using Rediter.Api.Repositories;
using Rediter.Api.Services.UtilitariesServices;

namespace Rediter.Api.Services
{
    public class PostUserLikeService : BaseService<UserPostLike>
    {
        private readonly UserPostLikeRepository userPostLikeRepository;

        public PostUserLikeService(UserPostLikeRepository userPostLikeRepository) : base(userPostLikeRepository)
        {
            this.userPostLikeRepository = userPostLikeRepository;
        }
    }
}
