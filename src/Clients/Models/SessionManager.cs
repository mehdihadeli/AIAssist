namespace Clients.Models;

public class SessionManager
{
    private List<Session> Sessions { get; set; } = new();
    private ChatHistory ChatHistory { get; set; } = new();

    // Create a new session (each session has a single chat)
    public Session CreateSession(string sessionName, User user, LLM assistant)
    {
        // sessionName is like a tab name and it is different with sessionId that will create internally by Session class
        var session = new Session(sessionName, user, assistant);
        Sessions.Add(session);
        return session;
    }

    // End the session and store chat history
    public void EndSession(string sessionId)
    {
        var session = GetSessionById(sessionId);
        if (session == null)
            throw new Exception("Session not found");

        // End the chat and add to chat history
        session.EndSession();
        var historyItem = new HistoryItem(
            session.Chat.Id,
            session.SessionId,
            session.Chat.StartTime,
            session.Chat.EndTime ?? DateTime.Now,
            session.Chat.ChatItems
        );
        ChatHistory.AddHistoryItem(historyItem);

        // Remove session from active sessions list
        Sessions.Remove(session);
    }

    // Get a session by its ID
    public Session? GetSessionById(string sessionId)
    {
        return Sessions.FirstOrDefault(session => session.SessionId == sessionId);
    }

    // Retrieve chat history for a specific session
    public IList<HistoryItem> GetSessionHistory(string sessionId)
    {
        return ChatHistory.GetHistoryForSession(sessionId);
    }
}
