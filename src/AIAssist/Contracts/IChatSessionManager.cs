using AIAssist.Chat.Models;

namespace AIAssist.Contracts;

public interface IChatSessionManager
{
    ChatSession CreateNewSession();
    void SetCurrentActiveSession(ChatSession? chatSession);
    ChatSession GetCurrentActiveSession();
    ChatSession GetSession(Guid chatSessionId);
    IEnumerable<ChatSession> GetAllSessions();
}
