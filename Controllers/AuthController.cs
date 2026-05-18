using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rediter.Api.DTOs;
using Rediter.Api.Services.UtilitariesServices;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Login com email e senha
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            try
            {
                TokenRequestDTO token = await _authService.AuthenticateFromRediter(login.Email, login.Password);
                return Ok(token);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Invalid", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("[Auth] Falha de credenciais para: {Email}", login.Email);
                    return Unauthorized(new { message = "Email ou senha incorretos." }); 
                }

                _logger.LogError(ex, "[Auth] Erro interno ao realizar login para: {Email}", login.Email);
                return StatusCode(500, new { message = "Erro interno ao realizar autenticação." });
            }
        }

        /// <summary>
        /// Login com Google OAuth
        /// </summary>
        [HttpPost("login/google")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDTO dto)
        {
            try
            {
                TokenRequestDTO token = await _authService.AuthenticateFromGoogle(dto.IdToken);
                return Ok(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Auth] Erro real ao autenticar via Google.");
                return Unauthorized(new { message = "Falha ao autenticar com o Google." });
            }
        }

        /// <summary>
        /// Envia código de verificação por email
        /// </summary>
        [HttpPost("verification-code")]
        [AllowAnonymous]
        public async Task<IActionResult> SendVerificationCode([FromBody] EmailRequestDTO request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest(new { message = "O email é obrigatório." });

                await _authService.SendVerificationCode(request.Email);
                return Ok(new { message = "Código enviado com sucesso." });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { message = "Usuário não encontrado." }); 

                _logger.LogError(ex, "[Auth] Erro ao enviar código para: {Email}", request.Email);
                return StatusCode(500, new { message = "Erro ao enviar código de verificação." });
            }
        }

        /// <summary>
        /// Verifica código enviado por email
        /// </summary>
        [HttpPost("verification-code/confirm")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmVerificationCode([FromBody] VerifyCodeRequestDTO request)
        {
            try
            {
                TokenRequestDTO token = await _authService.VerifyCode(request.Code, request.Email);
                return Ok(token);
            }
            catch (Exception ex)
            {
                // Erro do cliente (código errado), marcamos como Warning.
                _logger.LogWarning("[Auth] Código inválido para: {Email}. Motivo: {Motivo}", request.Email, ex.Message);
                return BadRequest(new { message = "Código inválido ou expirado." });
            }
        }

        /// <summary>
        /// Gera novo access token
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                    return BadRequest(new { message = "Refresh token é obrigatório." });

                var result = await _authService.RefreshTokenAsync(request.RefreshToken);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("[Auth] Tentativa de refresh token negada. Motivo: {Mensagem}", result.ErrorMessage);
                    return Unauthorized(new { message = result.ErrorMessage });
                }

                return Ok(result.Tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Auth] Erro interno ao atualizar token.");
                return StatusCode(500, new { message = "Erro ao atualizar sessão." });
            }
        }
    }
}