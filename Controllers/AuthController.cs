using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rediter.Api.DTOs;
using Rediter.Api.Services.UtilitariesServices;

namespace Rediter.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Login com email e senha
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] UserDTO user)
    {
        try
        {
            _logger.LogInformation(
                "[Auth] Tentativa de login para: {Email}",
                user.Email
            );

            if (string.IsNullOrWhiteSpace(user.Email) ||
                string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest(new
                {
                    message = "Email e senha são obrigatórios."
                });
            }

            TokenRequestDTO token =
                await _authService.AuthenticateFromRediter(
                    user.Email,
                    user.Password
                );

            _logger.LogInformation(
                "[Auth] Login realizado com sucesso para: {Email}",
                user.Email
            );

            return Ok(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[Auth] Erro ao realizar login para: {Email}",
                user.Email
            );

            return StatusCode(500, new
            {
                message = "Erro interno ao realizar autenticação."
            });
        }
    }

    /// <summary>
    /// Login com Google OAuth
    /// </summary>
    [HttpPost("login/google")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin(
        [FromBody] GoogleLoginRequestDTO dto)
    {
        try
        {
            _logger.LogInformation(
                "[Auth] Tentativa de login via Google."
            );

            TokenRequestDTO token =
                await _authService.AuthenticateFromGoogle(dto.IdToken);

            _logger.LogInformation(
                "[Auth] Login via Google realizado com sucesso."
            );

            return Ok(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[Auth] Erro ao autenticar via Google."
            );

            return StatusCode(500, new
            {
                message = "Falha ao autenticar com Google."
            });
        }
    }

    /// <summary>
    /// Envia código de verificação por email
    /// </summary>
    [HttpPost("verification-code")]
    [AllowAnonymous]
    public async Task<IActionResult> SendVerificationCode(
        [FromBody] string userEmail)
    {
        try
        {
            _logger.LogInformation(
                "[Auth] Enviando código de verificação para: {Email}",
                userEmail
            );

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return BadRequest(new
                {
                    message = "O email é obrigatório."
                });
            }

            await _authService.SendVerificationCode(userEmail);

            return Ok(new
            {
                message = "Código enviado com sucesso."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[Auth] Erro ao enviar código para: {Email}",
                userEmail
            );

            return StatusCode(500, new
            {
                message = "Erro ao enviar código de verificação."
            });
        }
    }

    /// <summary>
    /// Verifica código enviado por email
    /// </summary>
    [HttpPost("verification-code/confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmVerificationCode(
        [FromBody] VerifyCodeRequestDTO request)
    {
        try
        {
            _logger.LogInformation(
                "[Auth] Verificando código para: {Email}",
                request.Email
            );

            TokenRequestDTO token =
                await _authService.VerifyCode(
                    request.Code,
                    request.Email
                );

            return Ok(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[Auth] Código inválido para: {Email}",
                request.Email
            );

            return BadRequest(new
            {
                message = "Código inválido ou expirado."
            });
        }
    }

    /// <summary>
    /// Gera novo access token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken(
        [FromBody] string refreshToken)
    {
        try
        {
            _logger.LogInformation(
                "[Auth] Atualizando access token."
            );

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(new
                {
                    message = "Refresh token é obrigatório."
                });
            }

            var result =
                await _authService.RefreshTokenAsync(refreshToken);

            if (!result.IsSuccess)
            {
                return Unauthorized(new
                {
                    message = result.ErrorMessage
                });
            }

            return Ok(result.Tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[Auth] Erro ao atualizar token."
            );

            return StatusCode(500, new
            {
                message = "Erro ao atualizar sessão."
            });
        }
    }
}