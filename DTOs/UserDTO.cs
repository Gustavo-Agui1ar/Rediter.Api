using System.ComponentModel.DataAnnotations;

namespace Rediter.Api.DTOs
{
    public class UserDTO
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ImageName { get; set; }
        public string? ImageCover { get; set; }
    }
    public class UserUpdateDTO
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public IFormFile? File { get; set; }
        public IFormFile? Cover { get; set; }
        public string? RefreshToken { get; set; }
    }
}
