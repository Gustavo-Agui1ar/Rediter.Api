namespace Rediter.Api.DTOs
{
    public class UpdatePostDTO
    {
        public string? Text { get; set; }
        public IList<string>? RetainedPictures { get; set; }
        public IList<IFormFile>? Pictures { get; set; }
        public string? LocationName { get; set; }
    }
}
