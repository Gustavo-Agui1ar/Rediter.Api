namespace Rediter.Api.DTOs.Posts
{
    public record NewPostDTO(
        string? Text,
        IList<IFormFile>? Pictures,
        string? LocationName,
        string? ParentPostId
    );
}
