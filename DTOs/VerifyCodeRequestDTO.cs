namespace Rediter.Api.DTOs
{
    public class VerifyCodeRequestDTO
    {
        public string Code { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
