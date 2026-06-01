namespace Rediter.Api.DTOs
{
    public class CreateGroupDTO
    {
        public string Title { get; set; } = string.Empty;
        public List<Guid> ParticipantsIds { get; set; } = [];
    }
}
