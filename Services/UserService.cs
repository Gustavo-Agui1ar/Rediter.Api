using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Rediter.Api.DTOs;
using Rediter.Api.Models;
using Rediter.Api.Repositories;
using Rediter.Api.Services.UtilitariesServices;

namespace Rediter.Api.Services
{
    public class UserService : BaseService<User>
    {
        private readonly UserRepository _UserRepository;
        private readonly PictureService _pictureService;
        private readonly EmailService _emailService;
        private readonly UserFollowerService _userFollowerService;

        public UserService(UserRepository userRepository,
            EmailService emailService,
            PictureService pictureService,
            UserFollowerService userFollowerService) : base(userRepository)
        {
            _UserRepository = userRepository;
            _emailService = emailService;
            _pictureService = pictureService;
            _userFollowerService = userFollowerService;
        }

        public async Task<string> CreateUser(UserDTO dto)
        {
            using (var transaction = await _UserRepository.BeginTransaction())
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
                    await _UserRepository.Insert(user);
                    await transaction.CommitAsync();
                    return user.Id.ToString();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Ocorreu um erro ao criar o usuário: " + ex.Message);
                }
            }
        }

        public async Task<string> GetUserCodeAsync(string userId)
        {
            return await _UserRepository.GetCodeByIdAsync(userId);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _UserRepository.GetByEmailAsync(email);
        }

        public async Task<User?> GetUserByRefreshAsync(string refreshToken)
        {
            return await _UserRepository.GetUserByRefreshAsync(refreshToken);
        }

        public async Task<UserDTO> GetUserDto(User targetUser, Guid currentUserId)
        {
            if (targetUser == null)
                throw new Exception("User not found.");

            bool isFollowing = await _UserRepository.IsFollowingAsync(currentUserId, targetUser.Id);

            return new UserDTO
            {
                Name = targetUser.Name,
                Email = targetUser.Email,
                ImageName = targetUser.ProfilePicture?.FileName,
                ImageCover = targetUser.ProfileCover?.FileName,
                Description = targetUser.Description,
                IsFollowing = isFollowing 
            };
        }

        public async Task UpdateUserByUserDTO(UserUpdateDTO dto, User user)
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
            await _UserRepository.Update(user);
        }

        public async Task<bool> AddPictureFromGoogle(User user, string pictureUrl)
        {
            try
            {
                if (user == null)
                    throw new Exception("User not found.");

                user.ProfilePicture = await _pictureService.CreateImageFromGoogle(pictureUrl);
                user.ProfilePictureId = user.ProfilePicture!.Id;
                await _UserRepository.Update(user);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding profile picture from Google: " + ex.Message);
            }
        }

        public async Task<IList<UserFeedInfoDTO>> SearchUsers(string query, DateTime? lastCreatedAt, string? lastId, int pageSize, string userId)
        {
            return await _UserRepository.SearchUsers(query, lastCreatedAt, lastId, pageSize, userId);
        }

        public async Task FollowUser(string followerId, string followeeId)
        {
            try
            {
                User? follower = await _UserRepository.GetByUuid(new Guid(followerId));
                User? followee = await _UserRepository.GetByUuid(new Guid(followeeId));

                if (follower == null || followee == null)
                    throw new Exception("Follower or followee not found.");

                if (follower.Following.Any(f => f.FollowingId == followee.Id))
                    throw new Exception("Already following this user.");

                UserFollower follow = new UserFollower
                {
                    FollowerId = follower.Id,
                    FollowingId = followee.Id
                };

                follower.Following.Add(follow);
                followee.Followers.Add(follow);

                await _userFollowerService.InsertAsync(follow);
                await _UserRepository.Update(follower);
                await _UserRepository.Update(followee);
            }
            catch (Exception ex)
            {
                throw new Exception("Error following user: " + ex.Message);
            }
        }

        public async Task UnfollowUser(string followerId, string followeeId)
        {
            try
            {
                User? follower = await _UserRepository.GetByUuid(new Guid(followerId));
                User? followee = await _UserRepository.GetByUuid(new Guid(followeeId));

                if (follower == null || followee == null)
                    throw new Exception("Follower or followee not found.");

                UserFollower? follow = follower.Following.FirstOrDefault(f => f.FollowingId == followee.Id);

                if (follow == null)
                    throw new Exception("Not following this user.");

                follower.Following.Remove(follow);
                followee.Followers.Remove(follow);
                await _userFollowerService.DeleteAsync(follow);
                await _UserRepository.Update(follower);
                await _UserRepository.Update(followee);
            }
            catch (Exception ex)
            {
                throw new Exception("Error unfollowing user: " + ex.Message);
            }
        }
    }
}
