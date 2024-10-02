namespace Clients.Models;

public record User(string Id, string Name)
{
    public ChatItem GenerateUserPrompt(string prompt, string codeContext)
    {
        return new ChatItem(RoleType.System, prompt, codeContext);
    }
}
