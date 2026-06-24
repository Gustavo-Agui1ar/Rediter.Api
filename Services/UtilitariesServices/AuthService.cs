using Google.Apis.Auth;
using Rediter.Api.DTOs.Auth;
using Rediter.Api.Models.Users;
using Rediter.Api.Services.Users;

namespace Rediter.Api.Services.UtilitariesServices
{
    public class AuthService
    {
        private readonly UserService _userService;
        private readonly EmailService _emailService;
        private readonly TokenService _tokenService;
        private readonly string _googleClientId;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserService userService, TokenService tokenService, EmailService emailService, IConfiguration config, ILogger<AuthService> logger)
        {
            _userService = userService;
            _tokenService = tokenService;
            _emailService = emailService;
            _googleClientId = config["GoogleSettings:ClientId"]!;
            _logger = logger;
        }

        public async Task<TokenRequestDTO> VerifyCode(string inputCode, string userEmail)
        {
            User? user = await _userService.GetByEmailAsync(userEmail);

            if (user == null)
                throw new Exception("User not found");

            if(user.VerificationCodeExpiration == null)
                throw new Exception("Verification code not generated.");

            if (!inputCode.Equals(user.VerificationCode) || DateTime.UtcNow > user.VerificationCodeExpiration.Value)
                throw new Exception("Invalid or expired verification code.");

            user.VerificationCodeExpiration = null;
            user.IsVerified = true;

            return await GenerateToken(user, true);
        }

        public async Task SendVerificationCode(string email)
        {
            User? user = await _userService.GetByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found");

            string verificationCode = Random.Shared.Next(100000, 999999).ToString();

            user.VerificationCode = verificationCode;
            user.VerificationCodeExpiration = DateTime.UtcNow.AddMinutes(3);
            user.CreatedAt = DateTime.UtcNow; 

            _userService.Update(user);
            await _userService.SaveChangesAsync();

            await _emailService.SendVerificationCodeAsync(user.Email, user.Name, verificationCode, user.LanguageCode);
        }

        public async Task<TokenRequestDTO> AuthenticateFromRediter(string email, string password)
        {
            User? user = await _userService.GetByEmailAsync(email);

            if (user == null)
                throw new Exception("Invalid email.");

            if(!user.IsVerified)
                throw new Exception("User not verified.");

            if (!HashService.VerifyPassword(password, user.Password!))
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
            catch (InvalidJwtException ex)
            {
                _logger.LogError(ex, "[Auth] Erro real Google JWT");
                throw;
            }

            string userEmail = payload.Email;
            string userName = payload.Name;

            User? user = await _userService.GetByEmailAsync(userEmail);

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

                _userService.Insert(user);
                await _userService.SaveChangesAsync();

                if (!string.IsNullOrEmpty(payload.Picture))
                {
                    await _userService.AddPictureFromGoogle(user, payload.Picture);
                }
            }

            return await GenerateToken(user, true);
        }

        private async Task<TokenRequestDTO> GenerateToken(User user, bool addDays = false)
        {
            TokenRequestDTO dto = _tokenService.GenerateToken(user!);

            user.RefreshToken = dto.RefreshToken;

            if (addDays)
                user.RefreshTokenExpiration = DateTime.UtcNow.AddDays(30);

            _userService.Update(user);
            await _userService.SaveChangesAsync(); 

            return dto;
        }

        public async Task<(bool IsSuccess, string ErrorMessage, TokenRequestDTO? Tokens)> RefreshTokenAsync(string refreshToken)
        {
            User? user = await _userService.GetUserByRefreshAsync(refreshToken);

            if (user == null)
                return (false, "Invalid Refresh Token. Please login again.", null);

            if (user.RefreshTokenExpiration <= DateTime.UtcNow)
                return (false, "Refresh Token expired. Please login again.", null);

            TokenRequestDTO tokens = await GenerateToken(user);
            return (true, "", tokens);
        }

    }
}