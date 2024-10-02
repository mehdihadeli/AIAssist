namespace Clients.Models;

public class ChatHistory
{
    public IList<HistoryItem> HistoryItems { get; set; } = new List<HistoryItem>();

    public void AddHistoryItem(HistoryItem historyItem)
    {
        HistoryItems.Add(historyItem);
    }

    public IList<HistoryItem> GetHistoryForSession(string sessionId)
    {
        return HistoryItems.Where(item => item.SessionId == sessionId).ToList();
    }
}
