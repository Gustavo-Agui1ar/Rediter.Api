using Rediter.Api.Data;
using Rediter.Api.Models;
using System.Security.Cryptography.X509Certificates;

namespace Rediter.Api.Repositories
{
    public class PostImageRepository : BaseRepository<Models.PostImage>
    {
        public PostImageRepository(DataContext data) : base(data)
        {
           
        }

    }
}
