namespace Rediter.Api.DTOs
{
    public class NewPostDTO
    {
        public string? Text { get; set; }
        public IList<IFormFile>? Pictures { get; set; }
        public string? LocationName { get; set; }
    }
}
