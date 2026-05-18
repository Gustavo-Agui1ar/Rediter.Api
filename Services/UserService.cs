using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Rediter.Api.DTOs;
using Rediter.Api.Models;
using Rediter.Api.Repositories;
using Rediter.Api.Services.UtilitariesServices;
using System.Transactions;

namespace Rediter.Api.Services
{
    public class UserService : BaseService<User>
    {
        private readonly UserRepository _UserRepository;
        private readonly PictureService _pictureService;
        private readonly EmailService _emailService;
        private readonly UserFollowerService _userFollowerService;
        private readonly UserBlockService _userBlockService;
        private readonly PostService _postService;

        public UserService(UserRepository userRepository,
            EmailService emailService,
            PictureService pictureService,
            UserFollowerService userFollowerService,
            UserBlockService userBlockService,
            PostService postService) : base(userRepository)
        {
            _UserRepository = userRepository;
            _emailService = emailService;
            _pictureService = pictureService;
            _userFollowerService = userFollowerService;
            _userBlockService = userBlockService;
            _postService = postService; 
        }

        public async Task<string> CreateUser(UserDTO dto)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                        throw new Exception("Nome, e-mail e senha são obrigatórios.");

                    User user = new User
                    {
                        Name = dto.Name,
                        Email = dto.Email,
                        Password = HashService.HashPassword(dto.Password),
                        CreatedAt = DateTime.UtcNow,
                        VerificationCode = new Random(DateTime.Now.Millisecond).Next(100000, 999999).ToString()
                    };

                    await _emailService.SendVerificationCodeAsync(user.Email, user.Name, user.VerificationCode);

                    _UserRepository.Insert(user);

                    await SaveChangesAsync();
                    scope.Complete();
                    return user.Id.ToString();
                }
                catch (Exception ex)
                {
                    throw new Exception("Ocorreu um erro ao criar o usuário: " + ex.Message);
                }
            }
        }

        public async Task<string> GetUserCodeAsync(string userId)
        {
            return await _UserRepository.GetCodeByIdAsync(Guid.Parse(userId));
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _UserRepository.GetByEmailAsync(email);
        }

        public async Task<User?> GetUserByRefreshAsync(string refreshToken)
        {
            return await _UserRepository.GetUserByRefreshAsync(refreshToken);
        }

        public async Task<UserDTO> GetUserProfileAsync(Guid targetUserId, Guid currentUserId)
        {
            return await _UserRepository.GetUserProfileAsync(targetUserId, currentUserId);
        }

        public async Task UpdateUserByUserDTO(UserUpdateDTO dto, User user)
        {
            try
            {
                if (user == null)
                    throw new Exception("User not found.");

                if (!string.IsNullOrWhiteSpace(dto.Name))
                    user.Name = dto.Name;

                if (!string.IsNullOrWhiteSpace(dto.Email))
                    user.Email = dto.Email;

                if (!string.IsNullOrWhiteSpace(dto.Password))
                    user.Password = HashService.HashPassword(dto.Password);

                if (!string.Equals(user.Description, dto.Description))
                    user.Description = dto.Description;

                if (dto.File != null)
                {
                    user.ProfilePicture = await _pictureService.CreatePicture(dto.File);
                    user.ProfilePictureId = user.ProfilePicture.Id;
                }

                if (dto.Cover != null)
                {
                    user.ProfileCover = await _pictureService.CreatePicture(dto.Cover);
                    user.ProfileCoverId = user.ProfileCover.Id;
                }

                _UserRepository.Update(user);
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating user: " + ex.Message);
            }
        }

        public async Task<bool> AddPictureFromGoogle(User user, string pictureUrl)
        {
            try
            {
                if (user == null)
                    throw new Exception("User not found.");

                user.ProfilePicture = await _pictureService.CreateImageFromGoogle(pictureUrl);
                user.ProfilePictureId = user.ProfilePicture!.Id;

                _UserRepository.Update(user);
                await SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding profile picture from Google: " + ex.Message);
            }
        }

        public async Task<IList<UserFeedInfoDTO>> SearchUsers(string query, DateTime? lastCreatedAt, Guid? lastId, int pageSize, Guid userId)
        {
            return await _UserRepository.SearchUsers(query, lastCreatedAt, lastId, pageSize, userId);
        }

        public async Task FollowUser(Guid followerId, Guid followeeId)
        {
            try
            {
                bool alreadyFollowing = await _UserRepository.IsFollowingAsync(followerId, followeeId);
                if (alreadyFollowing)
                    throw new Exception("Already following this user.");

                User? follower = await _UserRepository.GetByUuid(followerId);
                User? followee = await _UserRepository.GetByUuid(followeeId);

                if (follower == null || followee == null)
                    throw new Exception("Follower or followee not found.");

                UserFollower follow = new UserFollower
                {
                    FollowerId = follower.Id,
                    FollowingId = followee.Id
                };

                follower.FollowingCount++;
                followee.FollowersCount++;

                _userFollowerService.Insert(follow);
                _UserRepository.Update(follower);
                _UserRepository.Update(followee);

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
                UserFollower? follow = await _userFollowerService.GetRelationAsync(followerId, followeeId);

                if (follow == null)
                    throw new Exception("Not following this user.");

                User? follower = await _UserRepository.GetByUuid(followerId);
                User? followee = await _UserRepository.GetByUuid(followeeId);

                if (follower == null || followee == null)
                    throw new Exception("Follower or followee not found.");

                if (follower.FollowingCount > 0) follower.FollowingCount--;
                if (followee.FollowersCount > 0) followee.FollowersCount--;

                _userFollowerService.Delete(follow);

                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error unfollowing user: " + ex.Message);
            }
        }

        public async Task BlockUser(Guid user, Guid targetBlock)
        {
            try
            {
                User? blocker = await _UserRepository.GetByUuid(user);
                User? blocked = await _UserRepository.GetByUuid(targetBlock);
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

                _userBlockService.Insert(block);
                _UserRepository.Update(blocker);
                _UserRepository.Update(blocked);

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
                User? blocker = await _UserRepository.GetByUuid(user);
                User? blocked = await _UserRepository.GetByUuid(targetUnblock);

                if (blocker == null || blocked == null)
                    throw new Exception("Blocker or blocked user not found.");

                UserBlock? block = blocker.BlockedUsers.FirstOrDefault(b => b.BlockedId == blocked.Id);

                if (block == null)
                    throw new Exception("Not blocking this user.");

                _userBlockService.Delete(block);

                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error unblocking user: " + ex.Message);
            }
        }

        public async Task<IList<UserFeedInfoDTO>> GetBlockedUsers(DateTime? lastCreatedAt, Guid? lastId, int pageSize, Guid currentUserID)
        {
            return await _UserRepository.GetBlockedUsers(lastCreatedAt, lastId, pageSize, currentUserID);
        }

        public async  Task<IList<PostFeedDTO>> GetLikedPosts(DateTime? lastCreatedAt, Guid? lastId, int pageSize, Guid currentUserId)
        {
            return await _postService.GetLikedPostsByUser(currentUserId, lastCreatedAt, lastId, pageSize);
        }
    }
}