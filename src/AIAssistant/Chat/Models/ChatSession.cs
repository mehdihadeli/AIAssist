using Clients.Models;

namespace AIAssistant.Chat.Models;

/// <summary>
/// Managing a single chat session and its history
/// </summary>
public class ChatSession
{
    public ChatSession()
    {
        ChatHistory = new ChatHistory(this);
    }

    public ChatHistory ChatHistory { get; }
    public Guid SessionId { get; } = Guid.NewGuid();

    public ChatItem AddUserChatItem(string prompt)
    {
        CheckForSystemContext();

        var chatItem = new ChatItem(RoleType.User, prompt);
        ChatHistory.AddToHistory(chatItem);

        return chatItem;
    }

    public ChatItem TrySetSystemContext(string systemContext)
    {
        var systemChatHistory = ChatHistory.HistoryItems.FirstOrDefault(x => x.Role != RoleType.System);
        if (systemChatHistory is not null)
        {
            return new ChatItem(RoleType.System, systemContext);
        }

        var chatItem = new ChatItem(RoleType.System, systemContext);
        ChatHistory.AddToHistory(chatItem);

        return chatItem;
    }

    public ChatItem AddAssistantChatItem(
        string prompt,
        int inputTokenCount,
        decimal inputCostPerToken,
        int outputTokenCount,
        decimal outputCostPerToken
    )
    {
        CheckForSystemContext();

        var chatItem = new ChatItem(RoleType.Assistant, prompt);
        ChatHistory.AddToHistory(chatItem, inputTokenCount, inputCostPerToken, outputTokenCount, outputCostPerToken);

        return chatItem;
    }

    public IList<ChatItem> GetChatItemsFromHistory()
    {
        var chatItems = new List<ChatItem>();

        var chatHistoryItems = ChatHistory.HistoryItems;

        if (chatHistoryItems.Any())
        {
            var historyItem = chatHistoryItems.Select(x => new ChatItem(x.Role, x.Prompt));
            chatItems.AddRange(historyItem);
        }

        return chatItems;
    }

    private void CheckForSystemContext()
    {
        if (ChatHistory.HistoryItems.All(x => x.Role != RoleType.System))
        {
            throw new Exception("There is no system context in the current session");
        }
    }
}
