using AIAssistant.Chat.Models;
using AIAssistant.Models;

namespace AIAssistant.Contracts;

public interface IChatSessionManager
{
    ChatSession CreateNewSession();
    void SetCurrentActiveSession(ChatSession? chatSession);
    ChatSession GetCurrentActiveSession();
    ChatSession GetSession(Guid chatSessionId);
    IEnumerable<ChatSession> GetAllSessions();
}
