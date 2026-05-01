using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rediter.Api.DTOs;
using Rediter.Api.Services.UtilitariesServices;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("Auth")] // Mantive "Auth" para não quebrar a URL base do seu front-end
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("rediter")]
        public async Task<IActionResult> RediterAuth([FromBody] UserDTO user)
        {
            try
            {
                _logger.LogInformation("[Auth] Iniciando tentativa de login Rediter para: {Email}", user.Email);

                if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
                {
                    _logger.LogWarning("[Auth] Login rejeitado: Email ou senha em branco.");
                    return BadRequest(new { message = "Email e senha são obrigatórios." });
                }

                TokenRequestDTO token = await _authService.AuthenticateFromRediter(user.Email, user.Password);

                _logger.LogInformation("[Auth] Login Rediter realizado com sucesso para: {Email}", user.Email);
                return Ok(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Auth] Erro interno durante o login Rediter para {Email}.", user.Email);
                return StatusCode(500, new { message = "Ocorreu um erro interno ao processar a autenticação." });
            }
        }

        [HttpPost("google")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleAuth([FromBody] GoogleLoginRequestDTO dto)
        {
            try
            {
                _logger.LogInformation("[Auth] Iniciando autenticação via Google.");

                TokenRequestDTO token = await _authService.AuthenticateFromGoogle(dto.IdToken);

                _logger.LogInformation("[Auth] Login via Google realizado com sucesso.");
                return Ok(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Auth] Erro ao tentar autenticar via Google.");
                return StatusCode(500, new { message = "Falha ao autenticar com o Google." });
            }
        }

        [HttpPost("generate-code")]
        public async Task<IActionResult> GenerateCode([FromBody] string userEmail)
        {
            try
            {
                _logger.LogInformation("[Auth] Solicitando geração de código para: {Email}", userEmail);

                if (string.IsNullOrWhiteSpace(userEmail))
                    return BadRequest(new { message = "O email é obrigatório." });

                await _authService.SendVerificationCode(userEmail);

                _logger.LogInformation("[Auth] Código de verificação enviado com sucesso para: {Email}", userEmail);
                return Ok(new { message = "Código de verificação enviado para o e-mail." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Auth] Erro ao gerar/enviar código de verificação para {Email}.", userEmail);
                return StatusCode(500, new { message = "Erro ao enviar o código de verificação." });
            }
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequestDTO request)
        {
            try
            {
                _logger.LogInformation("[Auth] Tentativa de verificação de código para: {Email}", request.Email);

                TokenRequestDTO token = await _authService.VerifyCode(request.Code, request.Email);

                _logger.LogInformation("[Auth] Código verificado com sucesso para: {Email}", request.Email);
                return Ok(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Auth] Erro ao verificar código para {Email}.", request.Email);
                return BadRequest(new { message = "Código inválido ou expirado." });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            try
            {
                _logger.LogInformation("[Auth] Tentativa de atualização de token (Refresh Token).");

                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    _logger.LogWarning("[Auth] Refresh token recebido está vazio.");
                    return BadRequest(new { message = "O Refresh Token é obrigatório." });
                }

                var result = await _authService.RefreshTokenAsync(refreshToken);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("[Auth] Falha ao atualizar token. Motivo: {ErrorMessage}", result.ErrorMessage);
                    return Unauthorized(new { message = result.ErrorMessage });
                }

                _logger.LogInformation("[Auth] Token atualizado com sucesso.");
                return Ok(result.Tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Auth] Erro interno durante o Refresh Token.");
                return StatusCode(500, new { message = "Erro ao tentar atualizar a sessão." });
            }
        }
    }
}