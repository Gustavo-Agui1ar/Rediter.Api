using Rediter.Api.Data;
using Rediter.Api.Models.Post;
using System.Security.Cryptography.X509Certificates;

namespace Rediter.Api.Repositories
{
    public class PostImageRepository : BaseRepository<PostImage>
    {
        public PostImageRepository(DataContext data) : base(data)
        {
           
        }

    }
}
