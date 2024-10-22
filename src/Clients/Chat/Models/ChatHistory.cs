using System.Text;
using Clients.Models;
using Humanizer;

namespace Clients.Chat.Models;

public class ChatHistory(Guid sessionId)
{
    private readonly IList<ChatHistoryItem> _historyItems = new List<ChatHistoryItem>();

    public IReadOnlyList<ChatHistoryItem> HistoryItems => _historyItems.AsReadOnly();
    public Guid SessionId { get; private set; } = sessionId;

    public void AddToHistory(ChatItem chatItem)
    {
        _historyItems.Add(new ChatHistoryItem(chatItem.Role, chatItem.Prompt));
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

    /// <summary>
    /// Gets a token that represents the combined value of all user tokens.
    /// </summary>
    public Token UserMessagesToken
    {
        get
        {
            var userPrompts = _historyItems
                .Where(x => x.Role == RoleType.User)
                .OrderBy(x => x.Created)
                .Select(x => x.Prompt);

            return new Token(string.Join(Environment.NewLine, userPrompts));
        }
    }

    /// <summary>
    /// Gets a token that represents the combined value of all system tokens.
    /// </summary>
    public Token SystemMessagesToken
    {
        get
        {
            var systemToken = _historyItems
                .Where(x => x.Role == RoleType.System)
                .OrderBy(x => x.Created)
                .Select(x => x.Prompt);

            return new Token(string.Join(Environment.NewLine, systemToken));
        }
    }

    /// <summary>
    /// Gets a token that represents the combined value of all assistant tokens.
    /// </summary>
    public Token AssistantMessagesToken
    {
        get
        {
            var assistantToken = _historyItems
                .Where(x => x.Role == RoleType.Assistant)
                .OrderBy(x => x.Created)
                .Select(x => x.Prompt);

            return new Token(string.Join(Environment.NewLine, assistantToken));
        }
    }

    /// <summary>
    /// Gets a token that represents the combined value of all tokens, ordered by the time they were created.
    /// </summary>
    public Token ContextToken
    {
        get
        {
            var contextToken = _historyItems.OrderBy(x => x.Created).Select(x => x.Prompt);

            return new Token(string.Join(Environment.NewLine, contextToken));
        }
    }

    /// <summary>
    /// Gets a token that represents the combined value of all user and assistant tokens,
    /// ordered by the time they were created.
    /// </summary>
    public Token ChatHistoryToken
    {
        get
        {
            var chatHistoryToken = _historyItems
                .Where(x => x.Role == RoleType.Assistant || x.Role == RoleType.User)
                .OrderBy(x => x.Created)
                .Select(x => x.Prompt);

            return new Token(string.Join(Environment.NewLine, chatHistoryToken));
        }
    }
}
