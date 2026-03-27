using Rediter.Api.DTOs;
using Rediter.Api.Models;
using Rediter.Api.Repositories;

namespace Rediter.Api.Services
{
    public class UserService
    {
        private readonly UserRepository _UserRepository;
        public UserService(UserRepository userRepository)
        {
            _UserRepository = userRepository;
        }

        public async Task<bool> CreateUser(UserDTO dto)
        {
            try
            {
                User user = new User
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    Password = HashService.HashPassword(dto.Password),
                    CreatedAt = DateTime.UtcNow
                };

                return await _UserRepository.Insert(user);
            }
            catch (Supabase.Postgrest.Exceptions.PostgrestException ex)
            {
                if (ex.Message.Contains("23505") || ex.Message.Contains("already exists"))
                {
                    if (ex.Message.Contains("users_name_key"))
                        throw new Exception("Este nome de usuário já está em uso.");

                    if (ex.Message.Contains("users_email_key"))
                        throw new Exception("Este e-mail já está cadastrado.");

                    throw new Exception("Usuário ou e-mail já existem.");
                }

                throw;
            }
        }

        public bool DeleteUser(string userId)
        {
            return _UserRepository.DeleteUser(userId);
        }
    }
}
