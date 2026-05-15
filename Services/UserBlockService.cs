using Rediter.Api.Models;
using Rediter.Api.Repositories;
using Rediter.Api.Services.UtilitariesServices;

namespace Rediter.Api.Services
{
    public class UserBlockService : BaseService<UserBlock>
    {
        private readonly UserBlockRepository userBlockRepository;

        public UserBlockService(UserBlockRepository userBlockRepository) : base(userBlockRepository)
        {
            this.userBlockRepository = userBlockRepository;
        }
    }
}
