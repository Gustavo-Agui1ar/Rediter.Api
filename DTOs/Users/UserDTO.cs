namespace Rediter.Api.DTOs.Users
{
    public record UserDTO
    {
        public Guid UserID { get; init; }
        public string? Name { get; init; }
        public string? Email { get; init; }
        public string? Password {  get; init; }
        public string? ImageName { get; init; }
        public string? ImageCover { get; init; }
        public string? Description { get; init; }
        public bool IsFollowing { get; init; }
        public bool isBlocked { get; set; }
        public int Followers { get; set; }
        public int Following { get; set; }
        public Guid? ChatId { get; set; }
    };

    public record UserUpdateDTO(
        string? Name = null,
        string? Email = null,
        string? Password = null,
        string? Description = null,
        string? Lan = null,
        IFormFile? File = null,
        IFormFile? Cover = null
    );
}
