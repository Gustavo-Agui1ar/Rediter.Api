using Rediter.Api.Data;

namespace Rediter.Api.Repositories
{
    public class Postrepository : BaseRepository<Models.Post>
    {
        public Postrepository(DataContext data) : base(data)
        {

        }
    }
}
