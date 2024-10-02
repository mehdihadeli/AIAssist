namespace Clients.Models;

public record LLM(string Id, string Name)
{
    public ChatItem GenerateResponse(string prompt)
    {
        return new ChatItem(RoleType.Assistant, prompt, null);
    }

    public ChatItem GenerateSystemPrompt(string prompt, string codeContext)
    {
        return new ChatItem(RoleType.System, prompt, codeContext);
    }
}
