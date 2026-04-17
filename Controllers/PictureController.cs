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
        public async Task<IActionResult> GetPicture([FromQuery] string name)
        {
            var stream = await _pictureService.GetPictureStream(name);
            if (stream == null)
                return NotFound("Imagem não encontrada.");

            return File(stream, "application/octet-stream", name);
        }
    }
}
