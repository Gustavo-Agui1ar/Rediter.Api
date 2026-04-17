using Rediter.Api.Data;
using Rediter.Api.Models;

namespace Rediter.Api.Repositories
{
    public class PictureRepository : BaseRepository<Models.Picture>
    {
        public PictureRepository(DataContext data) : base(data)
        {
            
        }
    }
}
