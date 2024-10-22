using Clients.Models;

namespace Clients.Chat.Models;

/// <summary>
/// Managing a single chat session and its history
/// </summary>
public class ChatSession
{
    public ChatSession()
    {
        ChatHistory = new ChatHistory(SessionId);
    }

    public ChatHistory ChatHistory { get; }
    public Guid SessionId { get; } = Guid.NewGuid();

    public ChatItem CreateUserChatItem(string prompt)
    {
        var chatItem = new ChatItem(RoleType.User, prompt);
        ChatHistory.AddToHistory(chatItem);

        return chatItem;
    }

    public ChatItem CreateSystemChatItem(string prompt)
    {
        var chatItem = new ChatItem(RoleType.System, prompt);
        ChatHistory.AddToHistory(chatItem);

        return chatItem;
    }

    public ChatItem CreateAssistantChatItem(string prompt)
    {
        var chatItem = new ChatItem(RoleType.Assistant, prompt);
        ChatHistory.AddToHistory(chatItem);

        return chatItem;
    }
}
