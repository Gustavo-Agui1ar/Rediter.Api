using Rediter.Api.DTOs;
using Rediter.Api.Models;
using Rediter.Api.Repositories;
using Rediter.Api.Services.UtilitariesServices;

namespace Rediter.Api.Services
{
    public class UserBlockService : BaseService<UserBlock>
    {
        private readonly UserBlockRepository _userBlockRepository;
        private readonly UserRepository _userRepository;

        public UserBlockService(UserBlockRepository userBlockRepository, UserRepository userRepository) : base(userBlockRepository)
        {
            this._userBlockRepository = userBlockRepository;
            this._userRepository = userRepository;
        }

        public async Task BlockUser(Guid user, Guid targetBlock)
        {
            try
            {
                User? blocker = await _userRepository.GetByUuid(user);
                User? blocked = await _userRepository.GetByUuid(targetBlock);
                if (blocker == null || blocked == null)
                    throw new Exception("Blocker or blocked user not found.");
                if (blocker.BlockedUsers.Any(b => b.BlockedId == blocked.Id))
                    throw new Exception("Already blocking this user.");

                UserBlock block = new UserBlock
                {
                    BlockerId = blocker.Id,
                    BlockedId = blocked.Id
                };

                blocker.BlockedUsers.Add(block);
                blocked.BlockedBy.Add(block);

                _userBlockRepository.Insert(block);
                _userRepository.Update(blocker);
                _userRepository.Update(blocked);

                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error blocking user: " + ex.Message);
            }
        }

        public async Task UnblockUser(Guid user, Guid targetUnblock)
        {
            try
            {
                User? blocker = await _userRepository.GetByUuid(user);
                User? blocked = await _userRepository.GetByUuid(targetUnblock);

                if (blocker == null || blocked == null)
                    throw new Exception("Blocker or blocked user not found.");

                UserBlock? block = blocker.BlockedUsers.FirstOrDefault(b => b.BlockedId == blocked.Id);

                if (block == null)
                    throw new Exception("Not blocking this user.");

                _userBlockRepository.Delete(block);

                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error unblocking user: " + ex.Message);
            }
        }

        public async Task<IList<UserFeedInfoDTO>> GetBlockedUsers(DateTime? lastCreatedAt, Guid? lastId, int pageSize, Guid currentUserID)
        {
            return await _userRepository.GetBlockedUsers(lastCreatedAt, lastId, pageSize, currentUserID);
        }
    }
}
