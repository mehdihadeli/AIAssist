namespace Clients.Models;

public class Token
{
    public TokenItem SystemMessagesToken { get; set; } = default!;
    public TokenItem UserMessagesToken { get; set; } = default!;
    public TokenItem HistoryToken { get; set; } = default!;

    public int TotalToken => SystemMessagesToken.Count + UserMessagesToken.Count + HistoryToken.Count;
}

public record TokenItem(string Value, int Count = 1);
