namespace Rediter.Api.DTOs.Auth
{
    public class VerifyCodeRequestDTO
    {
        public string Code { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
