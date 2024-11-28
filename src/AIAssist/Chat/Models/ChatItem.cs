using Clients.Models;

namespace AIAssist.Chat.Models;

public record ChatItem(RoleType Role, string Prompt)
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
