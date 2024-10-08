using System.Text;
using BuildingBlocks.LLM;
using Humanizer;
using Microsoft.Extensions.Primitives;

namespace Clients.Models;

public class ChatHistory(Guid sessionId)
{
    private readonly IList<HistoryItem> _historyItems = new List<HistoryItem>();

    public IReadOnlyList<HistoryItem> HistoryItems => _historyItems.AsReadOnly();
    public Guid SessionId { get; private set; } = sessionId;

    public Token Token { get; } = new();

    public void AddToHistory(ChatItem chatItem)
    {
        UpdateTokenValues(chatItem);
        _historyItems.Add(new HistoryItem(chatItem.Role, chatItem.Prompt));
    }

    public string GetFormattedHistory()
    {
        var history = new StringBuilder();
        foreach (var item in _historyItems)
        {
            history.AppendLine($"{item.Role.Humanize().Transform(To.LowerCase)}: {item.Prompt}");
        }

        return history.ToString();
    }

    public void UpdateTokenValues(ChatItem chatItem)
    {
        switch (chatItem.Role)
        {
            case RoleType.System:
                var systemValue = Token.SystemMessagesToken + chatItem.Prompt;
                Token.SystemMessagesToken = new TokenItem(systemValue, TokenizerHelper.GPT4TokenCount(systemValue));
                break;
            case RoleType.User:
                var userValue = Token.UserMessagesToken + chatItem.Prompt;
                Token.SystemMessagesToken = new TokenItem(userValue, TokenizerHelper.GPT4TokenCount(userValue));
                break;
        }

        var historyValue = Token.HistoryToken.Value + string.Concat(HistoryItems.Select(x => x.Prompt));
        Token.HistoryToken = new TokenItem(historyValue, TokenizerHelper.GPT4TokenCount(historyValue));
    }
}
