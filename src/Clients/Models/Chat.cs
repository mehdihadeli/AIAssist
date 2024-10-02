namespace Clients.Models;

public class Chat(string sessionId, User user, LLM assistant)
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public IList<ChatItem> ChatItems { get; set; } = new List<ChatItem>();
    public User User { get; set; } = user;
    public LLM Assistant { get; set; } = assistant;
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime? EndTime { get; set; }
    public string SessionId { get; set; } = sessionId; // Chat tied to a single session

    public void AddChatItem(ChatItem chatItem)
    {
        ChatItems.Add(chatItem);
    }

    public void EndChat()
    {
        EndTime = DateTime.Now;
    }
}
