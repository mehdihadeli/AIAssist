using System.Text;
using Clients.Models;
using Humanizer;

namespace AIAssistant.Chat.Models;

public class ChatHistory(ChatSession chatSession)
{
    private readonly IList<ChatHistoryItem> _historyItems = new List<ChatHistoryItem>();

    public IList<ChatHistoryItem> HistoryItems => _historyItems;
    public ChatSession ChatSession { get; private set; } = chatSession;

    public void AddToHistory(
        ChatItem chatItem,
        int inputTokenCount,
        decimal inputCostPerToken,
        int outputTokenCount,
        decimal outputCostPerToken
    )
    {
        var lastUserInputHistory = _historyItems.LastOrDefault(x => x.Role == RoleType.User);
        if (lastUserInputHistory is not null)
        {
            lastUserInputHistory.ChatCost = new ChatCost(inputTokenCount, inputCostPerToken, 0, 0);
        }

        _historyItems.Add(
            new ChatHistoryItem
            {
                Role = chatItem.Role,
                Prompt = chatItem.Prompt,
                ChatCost = new ChatCost(inputTokenCount, inputCostPerToken, outputTokenCount, outputCostPerToken),
            }
        );
    }

    public void AddToHistory(ChatItem chatItem)
    {
        _historyItems.Add(
            new ChatHistoryItem
            {
                Role = chatItem.Role,
                Prompt = chatItem.Prompt,
                ChatCost = null,
            }
        );
    }

    public override string ToString()
    {
        var history = new StringBuilder();

        foreach (var item in _historyItems)
        {
            history.AppendLine($"{item.Role.Humanize().Transform(To.LowerCase)}: {item.Prompt}");
        }

        return history.ToString();
    }
}
