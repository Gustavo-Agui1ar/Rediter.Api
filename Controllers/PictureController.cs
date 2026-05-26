using Microsoft.AspNetCore.Mvc;
using Rediter.Api.Services;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("api/pictures")]
    public class PictureController : ControllerBase
    {
        private readonly PictureService _pictureService;
        private readonly ILogger<PictureController> _logger;

        public PictureController(PictureService pictureService, ILogger<PictureController> logger)
        {
            _pictureService = pictureService;
            _logger = logger;
        }

        /// <summary>
        /// Retorna uma imagem pelo nome
        /// </summary>
        [HttpGet("{name}")]
        [ResponseCache(Duration = 2592000, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetByName([FromRoute] string name, [FromRoute] bool isThumb = true)
        {
            try
            {
                _logger.LogDebug("[Picture] Buscando imagem: {ImageName}", name);

                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { message = "O nome da imagem é obrigatório." });
                }

                (Stream? stream, string? contentType) = await _pictureService.GetPictureStream(name, isThumb);

                if (stream is null || contentType is null)
                {
                    _logger.LogWarning("[Picture] Imagem não encontrada: {ImageName}", name);
                    return NotFound(new { message = "Imagem não encontrada." });
                }

                return File(stream, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Picture] Erro ao buscar imagem: {ImageName}", name);

                return StatusCode(500, new { message = "Erro interno ao buscar imagem." });
            }
        }
    }
}