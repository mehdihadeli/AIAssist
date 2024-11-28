using AIAssist.Chat.Models;
using AIAssist.Contracts;

namespace AIAssist.Services;

public class ChatSessionManager : IChatSessionManager
{
    private readonly Dictionary<Guid, ChatSession> _chatSessions = new();
    private ChatSession? _activeSession;

    public ChatSession CreateNewSession()
    {
        var session = new ChatSession();
        _chatSessions.TryAdd(session.SessionId, session);

        return session;
    }

    public void SetCurrentActiveSession(ChatSession? chatSession)
    {
        _activeSession = chatSession;
    }

    public ChatSession GetCurrentActiveSession()
    {
        if (_activeSession is null)
        {
            throw new Exception("There is no active session");
        }

        return _activeSession;
    }

    public ChatSession GetSession(Guid chatSessionId)
    {
        var session = _chatSessions.GetValueOrDefault(chatSessionId);

        if (session is null)
        {
            throw new Exception($"Session {chatSessionId.ToString()} not found.");
        }

        return session;
    }

    public IEnumerable<ChatSession> GetAllSessions()
    {
        return _chatSessions.Select(x => x.Value);
    }
}
