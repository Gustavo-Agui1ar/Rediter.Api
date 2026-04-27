using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Rediter.Api.Services;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("Picture")]
    public class PictureController : ControllerBase
    {
        private readonly PictureService _pictureService;

        public PictureController(PictureService pictureService)
        {
            _pictureService = pictureService;
        }

        [HttpGet("GetPicture")]
        public async Task<IActionResult> GetPicture([FromQuery] string name, [FromQuery] int compressionLevel = 100)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("O nome da imagem é obrigatório.");

            (Stream? stream, string? contentType)= await _pictureService.GetPictureStream(  name, compressionLevel);

            if (stream == null || contentType == null)
                return NotFound("Imagem não encontrada.");

            return File(stream, contentType, name);
        }
    }
}
