namespace Clients.Models;

public record ChatItem(RoleType Role, string Prompt, string? CodeContext)
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
