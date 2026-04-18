using Microsoft.AspNetCore.Mvc;
using Rediter.Api.DTOs;

namespace Rediter.Api.Controllers
{
    [ApiController]
    [Route("Post")]
    public class PostController : ControllerBase
    {
        [HttpPost("NewPost")]
        public async Task NewPost([FromBody] NewPostDTO dto)
        {

        }
    }
}
