using Jose;
using Rediter.Api.DTOs;
using Rediter.Api.Models;
using Rediter.Api.Repositories;

namespace Rediter.Api.Services.UtilitariesServices
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

        public async Task<TokenRequestDTO> VerifyCode(string inputCode, string userEmail)
        {
            User? user = await _userService.GetByEmail(userEmail);

            if (user == null)
                throw new Exception("User not found");

            if (!inputCode.Equals(user?.VerificationCode) && user?.CreatedAt < user?.CreatedAt.AddDays(1))
                throw new Exception("Invalid verification code.");

            return await GenerateToken(user);
        }

        public async Task<TokenRequestDTO> AuthenticateFromRediter(string email, string password)
        {
            User? user = await _userService.GetByEmail(email);

            if (user == null)
                throw new Exception("Invalid email.");

            if (!HashService.VerifyPassword(password, user.Password))
                throw new Exception("Invalid password.");

            return await GenerateToken(user);
        }

        private async Task<TokenRequestDTO> GenerateToken(User? user)
        {
            TokenRequestDTO dto = _tokenService.GenerateToken(user!);

            user?.RefreshToken = dto.RefreshToken;
            user?.RefreshTokenExpiration = DateTime.UtcNow.AddDays(30);
            await _userService.UpdateAsync(user!);
            return dto;
        }

        public async Task<(bool IsSuccess, string ErrorMessage, TokenRequestDTO? Tokens)> RefreshTokenAsync(string refreshToken)
        {
            User? user = await _userService.GetUserByRefreshToken(refreshToken);

            if (user == null)
                return (false, "Invalid Refresh Token. Please login again.", null);

            if (user.RefreshTokenExpiration <= DateTime.UtcNow)
                return (false, "Refresh Token expired. Please login again.", null);

            var tokens = await GenerateToken(user);
            return (true, "", tokens);
        }
    }
}
