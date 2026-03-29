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
            User? user = await _userService.GetByUUId(userId);

            if (!inputCode.Equals(user?.VerificationCode))
                throw new Exception("Invalid verification code.");

            TokenRequestDTO dto = _tokenService.GenerateToken(user);

            user.RefreshToken = dto.RefreshToken;
            await _userService.Update(user);
            return dto;
        }
    }
}
