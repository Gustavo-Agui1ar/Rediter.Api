namespace Rediter.Api.DTOs
{
    public record NewPostDTO(
        string? Text,
        IList<IFormFile>? Pictures,
        string? LocationName,
        string? ParentPostId
    );
}
