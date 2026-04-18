namespace Rediter.Api.DTOs
{
    public class NewPostDTO
    {
        public string RefreshToken { get; set; } = null!;
        public string Text { get; set; } = null!;
        public IList<IFormFile>? Pictures { get; set; }
        public string? LocationName { get; set; }
    }
}
