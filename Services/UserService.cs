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

        public UserService(UserRepository userRepository, EmailService emailService, PictureService pictureService) : base(userRepository)
        {
            _UserRepository = userRepository;
            _emailService = emailService;
            _pictureService = pictureService;
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

        public async Task<UserDTO> GetUserDto(User user)
        {
            if (user == null)
                throw new Exception("User not found.");

            string? file = null;
            string? cover = null;

            if (user.ProfilePicture != null)
                file = user.ProfilePicture.FileName;

            if (user.ProfileCover != null)
                cover = user.ProfileCover.FileName;

            return new UserDTO
            {
                Name = user.Name,
                Email = user.Email,
                ImageName = file,
                ImageCover = cover
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
    }
}
