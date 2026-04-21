using Google.Apis.Auth;
using Rediter.Api.DTOs;
using Rediter.Api.Models;

namespace Rediter.Api.Services.UtilitariesServices
{
    public class AuthService
    {
        private readonly UserService _userService;
        private readonly TokenService _tokenService;
        private readonly string _googleClientId;
        public AuthService(UserService userService, TokenService tokenService, IConfiguration config)
        {
            _userService = userService;
            _tokenService = tokenService;
            _googleClientId = config["GoogleSettings:ClientId"]!;
        }

        public async Task<TokenRequestDTO> VerifyCode(string inputCode, string userEmail)
        {
            User? user = await _userService.GetByEmail(userEmail);

            if (user == null)
                throw new Exception("User not found");

            if (!inputCode.Equals(user?.VerificationCode) && user?.CreatedAt < user?.CreatedAt.AddDays(1))
                throw new Exception("Invalid verification code.");

            return await GenerateToken(user, true);
        }

        public async Task<TokenRequestDTO> AuthenticateFromRediter(string email, string password)
        {
            User? user = await _userService.GetByEmail(email);

            if (user == null)
                throw new Exception("Invalid email.");

            if (!HashService.VerifyPassword(password, user.Password))
                throw new Exception("Invalid password.");

            return await GenerateToken(user, true);
        }

        public async Task<TokenRequestDTO> AuthenticateFromGoogle(string idToken)
        {
            GoogleJsonWebSignature.Payload payload;

            try
            {
                var validationSettings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _googleClientId }
                };

                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
            }
            catch (InvalidJwtException)
            {
                throw new Exception("Token do Google inválido ou expirado.");
            }

            string userEmail = payload.Email;
            string userName = payload.Name;

            User? user = await _userService.GetByEmail(userEmail);

            if (user == null)
            {
                user = new User
                {
                    Email = userEmail,
                    Name = userName,
                    Password = "", 
                    VerificationCode = "",
                    CreatedAt = DateTime.UtcNow,
                };

                await _userService.AddPictureFromGoogle(user!, payload.Picture);
                await _userService.InsertAsync(user);
            }

            return await GenerateToken(user, true);
        }

        private async Task<TokenRequestDTO> GenerateToken(User? user, bool addDays = false)
        {
            TokenRequestDTO dto = _tokenService.GenerateToken(user!);

            user?.RefreshToken = dto.RefreshToken;

            if(addDays)
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

            TokenRequestDTO tokens = await GenerateToken(user);
            return (true, "", tokens);
        }
    }
}
