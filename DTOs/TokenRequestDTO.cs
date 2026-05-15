namespace Rediter.Api.DTOs
{
    public record TokenRequestDTO(
        string AccessToken = "",
        string RefreshToken = ""
    );
}
