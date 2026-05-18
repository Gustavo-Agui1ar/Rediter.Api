using Rediter.Api.Data;
using Rediter.Api.Models;
using System.Security.Cryptography.X509Certificates;

namespace Rediter.Api.Repositories
{
    public class NotificationRepository : BaseRepository<Notification>
    {
        public NotificationRepository(DataContext data) : base(data)
        {

        }

    }
}
