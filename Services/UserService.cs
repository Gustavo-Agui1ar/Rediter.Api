using Npgsql;
using Rediter.Api.DTOs;
using Rediter.Api.Models;
using Rediter.Api.Repositories;

namespace Rediter.Api.Services
{
    public class UserService
    {
        private readonly UserRepository _UserRepository;
        private readonly EmailService _emailService;

        public UserService(UserRepository userRepository, EmailService emailService)
        {
            _UserRepository = userRepository;
            _emailService = emailService;
        }

        public async Task<string> CreateUser(UserDTO dto)
        {
            try
            {
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
                return user.Id.ToString();
            }
            catch (Exception ex)
            {
                if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
                {
                    if (pgEx.ConstraintName != null && pgEx.ConstraintName.Contains("email"))
                    {
                        throw new Exception("Este e-mail já está em uso.");
                    }
                    if (pgEx.ConstraintName != null && pgEx.ConstraintName.Contains("name"))
                    {
                        throw new Exception("Este nome de usuário já está sendo utilizado.");
                    }

                    throw new Exception("Um registro com estes dados já existe.");
                }
                throw new Exception("Ocorreu um erro ao criar o usuário: " + ex.Message);
            }
        }

        public async Task<bool> DeleteUser(string userId)
        {
            User? user = await _UserRepository.GetByUUId(userId);

            if (user == null)
                throw new Exception("User not found.");

            return (await _UserRepository.Delete(user));
        }

        public string GetUserCode(string userId)
        {
            return _UserRepository.GetCodeById(userId);
        }

        public async Task<User?> GetByUUId(Guid id)
        {
            return await _UserRepository.GetByUUId(id);
        }

        public async Task<User?> GetByEmail(string email)
        {
            return await _UserRepository.GetByEmail(email);
        }

        public async Task<bool> Update(User user)
        {
            return await _UserRepository.Update(user);
        }
    }
}
