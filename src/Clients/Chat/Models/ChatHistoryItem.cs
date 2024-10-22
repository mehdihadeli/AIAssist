using Clients.Models;

namespace Clients.Chat.Models;

public record ChatHistoryItem(RoleType Role, string Prompt)
{
    public DateTime Created { get; } = DateTime.Now;
};
