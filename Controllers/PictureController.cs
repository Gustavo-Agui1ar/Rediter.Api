using Microsoft.AspNetCore.Mvc;
using Rediter.Api.Services;

namespace Rediter.Api.Controllers;

[ApiController]
[Route("api/pictures")]
public class PictureController : ControllerBase
{
    private readonly PictureService _pictureService;
    private readonly ILogger<PictureController> _logger;

    public PictureController(
        PictureService pictureService,
        ILogger<PictureController> logger)
    {
        _pictureService = pictureService;
        _logger = logger;
    }

    /// <summary>
    /// Retorna uma imagem pelo nome
    /// </summary>
    [HttpGet("{name}")]
    public async Task<IActionResult> GetByName(
        [FromRoute] string name)
    {
        try
        {
            _logger.LogInformation(
                "[Picture] Buscando imagem: {ImageName}",
                name
            );

            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new
                {
                    message = "O nome da imagem é obrigatório."
                });
            }

            (Stream? stream, string? contentType) =
                await _pictureService.GetPictureStream(name);

            if (stream is null || contentType is null)
            {
                _logger.LogWarning(
                    "[Picture] Imagem não encontrada: {ImageName}",
                    name
                );

                return NotFound(new
                {
                    message = "Imagem não encontrada."
                });
            }

            _logger.LogInformation(
                "[Picture] Imagem encontrada: {ImageName}",
                name
            );

            return File(
                fileStream: stream,
                contentType: contentType
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[Picture] Erro ao buscar imagem: {ImageName}",
                name
            );

            return StatusCode(500, new
            {
                message = "Erro interno ao buscar imagem."
            });
        }
    }
}