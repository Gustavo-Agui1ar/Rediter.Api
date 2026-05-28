using Rediter.Api.Models.Users;
using Rediter.Api.Repositories.Users;
using Rediter.Api.Services.UtilitariesServices;

namespace Rediter.Api.Services.Users
{
    public class UserFollowerService : BaseService<Follower>
    {
        private readonly FollowerRepository _userFollowerRepository;
        private readonly UserRepository _userRepository; 

        public UserFollowerService(
            FollowerRepository userFollowerRepository,
            UserRepository userRepository) : base(userFollowerRepository)
        {
            _userFollowerRepository = userFollowerRepository;
            _userRepository = userRepository;
        }

        public async Task<Follower?> GetRelationAsync(Guid followerId, Guid followingId)
        {
            return await _userFollowerRepository.GetRelationAsync(followerId, followingId);
        }

        public async Task FollowUser(Guid followerId, Guid followeeId)
        {
            try
            {
                bool alreadyFollowing = await GetRelationAsync(followerId, followeeId) != null;
                if (alreadyFollowing)
                    throw new Exception("Already following this user.");

                User? follower = await _userRepository.GetByUuid(followerId);
                User? followee = await _userRepository.GetByUuid(followeeId);

                if (follower == null || followee == null)
                    throw new Exception("Follower or followee not found.");

                Follower follow = new Follower
                {
                    FollowerId = follower.Id,
                    FollowingId = followee.Id
                };

                follower.FollowingCount++;
                followee.FollowersCount++;

                _userFollowerRepository.Insert(follow);
                _userRepository.Update(follower);
                _userRepository.Update(followee);

                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error following user: " + ex.Message);
            }
        }

        public async Task UnfollowUser(Guid followerId, Guid followeeId)
        {
            try
            {
                Follower? follow = await GetRelationAsync(followerId, followeeId);

                if (follow == null)
                    throw new Exception("Not following this user.");

                User? follower = await _userRepository.GetByUuid(followerId);
                User? followee = await _userRepository.GetByUuid(followeeId);

                if (follower == null || followee == null)
                    throw new Exception("Follower or followee not found.");

                if (follower.FollowingCount > 0) follower.FollowingCount--;
                if (followee.FollowersCount > 0) followee.FollowersCount--;

                _userFollowerRepository.Delete(follow);

                _userRepository.Update(follower);
                _userRepository.Update(followee);

                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error unfollowing user: " + ex.Message);
            }
        }
    }
}