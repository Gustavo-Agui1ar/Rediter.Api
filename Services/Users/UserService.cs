using Rediter.Api.DTOs;
using Rediter.Api.DTOs.Users;
using Rediter.Api.Models.Users;
using Rediter.Api.Repositories.Users;
using Rediter.Api.Services.UtilitariesServices;
using System.Transactions;

namespace Rediter.Api.Services.Users
{
    public class UserService : BaseService<User>
    {
        private readonly UserRepository _UserRepository;
        private readonly PictureService _pictureService;
        private readonly EmailService _emailService;

        public UserService(UserRepository userRepository,
            EmailService emailService,
            PictureService pictureService) : base(userRepository)
        {
            _UserRepository = userRepository;
            _emailService = emailService;
            _pictureService = pictureService;
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
                        VerificationCode = new Random(DateTime.Now.Millisecond).Next(100000, 999999).ToString(),
                        VerificationCodeExpiration = DateTime.UtcNow.AddMinutes(3)
                    };

                    await _emailService.SendVerificationCodeAsync(user.Email, user.Name, user.VerificationCode);

                    Role? defaultRole = await _UserRepository.GetRoleDefault();

                    if (defaultRole != null)
                        user.Roles.Add(defaultRole); 

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

        public async Task AddRole(User user, string roleName)
        {
            var role = await _UserRepository.GetRoleByNameAsync(roleName);
            if (role != null)
                user.Roles.Add(role);

            await SaveChangesAsync();
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
                
                if (!string.Equals(user.LanguageCode, dto.Lan) && !string.IsNullOrWhiteSpace(dto.Lan))
                    user.LanguageCode = dto.Lan;
    
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

        public async Task RegisterDeviceAsync(Guid userId, DeviceDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.deviceToken))
                return;

            User? user = await _UserRepository.GetByUuid(userId);

            if (user == null)
                return;

            if (user.DeviceToken == dto.deviceToken)
                return;

            user.DeviceToken = dto.deviceToken;

            await SaveChangesAsync();
        }

        public async Task<bool> DeleteUser(Guid userId, Guid currentUserId, bool isAdmin)
        {
            if (userId != currentUserId && !isAdmin) return false;

            try
            {
                await _UserRepository.DeleteUserAsync(userId);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}