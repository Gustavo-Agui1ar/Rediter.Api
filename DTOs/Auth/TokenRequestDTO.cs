namespace Rediter.Api.DTOs.Auth
{
    public record TokenRequestDTO(
        string AccessToken = "",
        string RefreshToken = ""
    );
}
