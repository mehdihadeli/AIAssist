namespace Clients.Models;

public class Session
{
    public string SessionId { get; set; }

    /// <summary>
    /// A name for the session (can represent tab name)
    /// </summary>
    public string SessionName { get; set; }
    public Chat Chat { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public Session(string sessionName, User user, LLM assistant)
    {
        SessionId = Guid.NewGuid().ToString();
        SessionName = sessionName;
        CreatedAt = DateTime.Now;
        Chat = new Chat(SessionId, user, assistant);
    }

    public void EndSession()
    {
        // End the chat when the session ends
        Chat.EndChat();
        ClosedAt = DateTime.Now;
    }
}
