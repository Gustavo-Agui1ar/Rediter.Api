using System.ComponentModel.DataAnnotations;

namespace Rediter.Api.DTOs
{
    public record UserDTO(
        string? Name = null,
        string? Email = null,
        string? Password = null,
        string? ImageName = null,
        string? ImageCover = null,
        string? Description = null,
        bool IsFollowing = false
    );

    public record UserUpdateDTO(
        string? Name = null,
        string? Email = null,
        string? Password = null,
        string? Description = null,
        IFormFile? File = null,
        IFormFile? Cover = null
    );
}
