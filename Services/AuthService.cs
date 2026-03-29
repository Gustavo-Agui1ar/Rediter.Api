using Jose;
using Rediter.Api.DTOs;
using Rediter.Api.Models;

namespace Rediter.Api.Services
{
    public class AuthService
    {
        private readonly UserService _userService;
        private readonly TokenService _tokenService;
        public AuthService(UserService userService, TokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        public async Task<TokenRequestDTO> VerifyCode(string inputCode, string userId)
        {
            Guid uuid = Guid.Parse(userId);
            User? user = await _userService.GetByUUId(uuid);

            if (user == null)
                throw new Exception("User not found");

            if (!inputCode.Equals(user?.VerificationCode) && user?.CreatedAt < user?.CreatedAt.AddDays(1))
                throw new Exception("Invalid verification code.");

            TokenRequestDTO dto = _tokenService.GenerateToken(user!);

            user?.RefreshToken = dto.RefreshToken;
            await _userService.Update(user!);
            return dto;
        }

        public async Task<TokenRequestDTO> AuthenticateFromRediter(string email, string password)
        {
            User? user = await _userService.GetByEmail(email);
            
            if (user == null)
                throw new Exception("Invalid email.");

            if (!HashService.VerifyPassword(password, user.Password))
                throw new Exception("Invalid password.");
            
            TokenRequestDTO dto = _tokenService.GenerateToken(user!);

            user?.RefreshToken = dto.RefreshToken;
            await _userService.Update(user!);
            
            return dto;
        }
    }
}
