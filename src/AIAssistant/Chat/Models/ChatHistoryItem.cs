using Clients.Models;

namespace AIAssistant.Chat.Models;

public record ChatHistoryItem
{
    public RoleType Role { get; set; }
    public string Prompt { get; set; } = default!;
    public ChatCost? ChatCost { get; set; } = default!;
    public DateTime Created { get; } = DateTime.Now;
}
