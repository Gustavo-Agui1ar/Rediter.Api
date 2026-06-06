using Microsoft.AspNetCore.Mvc;
using Rediter.Api.Services;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("api/pictures")]
    public class PictureController : ControllerBase
    {
        private readonly PictureService _pictureService;

        public PictureController(PictureService pictureService)
        {
            _pictureService = pictureService;
        }

        // GET: api/pictures/{name}?isThumb=true
        [HttpGet("{name}")]
        [ResponseCache(Duration = 2592000, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetByName([FromRoute] string name, [FromQuery] bool isThumb = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest(new { message = "O nome da imagem é obrigatório." });

                (Stream? stream, string? contentType) = await _pictureService.GetPictureStream(name, isThumb);

                if (stream is null || contentType is null)
                    return NotFound(new { message = "Imagem não encontrada." });

                return File(stream, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Erro interno ao buscar imagem: {ex.Message}" });
            }
        }
    }
}