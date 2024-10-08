namespace Clients.Models;

public record HistoryItem(RoleType Role, string Prompt)
{
    public DateTime Created { get; } = DateTime.Now;
};
