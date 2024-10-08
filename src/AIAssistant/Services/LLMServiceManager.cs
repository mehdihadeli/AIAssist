using AIAssistant.Contracts;
using AIAssistant.Prompts;
using Clients.Contracts;
using Clients.Models;

namespace AIAssistant.Services;

public class LLMServiceManager(ILLMClientStratgey llmClientStratgey) : ILLMServiceManager
{
    public Task<string?> GetCompletionAsync(ChatSession chatSession, string userQuery, string context)
    {
        var chatItems = new List<ChatItem>();

        var chatHistoryItems = chatSession.ChatHistory.HistoryItems;

        if (chatHistoryItems.Any())
        {
            var historyItem = chatHistoryItems.Select(x => new ChatItem(x.Role, x.Prompt));
            chatItems.AddRange(historyItem);
        }

        // if we don't have a `system` prompt in our chat history we should send it for llm otherwise we will read it from history
        if (chatSession.ChatHistory.HistoryItems.All(x => x.Role != RoleType.System))
        {
            var systemCodeAssistPrompt = PromptManager.RenderPromptTemplate(
                PromptConstants.CodeAssistantSimpleTemplate,
                new { codeContext = context }
            );
            var systemChatItem = chatSession.CreateSystemChatItem(systemCodeAssistPrompt);
            chatItems.Add(systemChatItem);
        }

        var userChatItem = chatSession.CreateUserChatItem(userQuery);
        chatItems.Add(userChatItem);

        return llmClientStratgey.GetCompletionAsync(chatItems);
    }

    public Task<IList<double>> GetEmbeddingAsync(string input)
    {
        return llmClientStratgey.GetEmbeddingAsync(input);
    }
}
