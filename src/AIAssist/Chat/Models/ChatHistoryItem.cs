using Clients.Models;

namespace AIAssist.Chat.Models;

public record ChatHistoryItem(string Prompt, RoleType Role, ChatCost? ChatCost)
{
    public RoleType Role { get; } = Role;
    public string Prompt { get; private set; } = Prompt;
    public ChatCost? ChatCost { get; private set; } = ChatCost;
    public DateTime Created { get; } = DateTime.Now;

    public void ChangeCost(ChatCost? chatCost)
    {
        ChatCost = chatCost;
    }

    public void ChangePrompt(string prompt)
    {
        Prompt = prompt;
    }
}
