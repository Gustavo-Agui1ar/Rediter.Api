namespace Rediter.Api.DTOs
{
    public record UpdatePostDTO (
        string? Text = null,
        IList<string>? RetainedPictures = null,
        IList<IFormFile>? Pictures = null,
        string? LocationName = null
    );
}
