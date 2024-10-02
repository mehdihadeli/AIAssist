namespace Clients.Models;

public class HistoryItem(
    string chatId,
    string sessionId,
    DateTime startTime,
    DateTime endTime,
    IList<ChatItem> chatItems
)
{
    public string ChatId { get; set; } = chatId;
    public string SessionId { get; set; } = sessionId; // Links history item to a specific session
    public DateTime ChatStartTime { get; set; } = startTime;
    public DateTime ChatEndTime { get; set; } = endTime;
    public IList<ChatItem> ChatItems { get; set; } = chatItems;
}
