using AIAssistant.Contracts;
using AIAssistant.Models;
using AIAssistant.Prompts;
using Clients.Chat.Models;
using Clients.Contracts;
using Clients.Models;
using Clients.Options;
using Microsoft.Extensions.Options;

namespace AIAssistant.Services;

public class LLMClientManager : ILLMClientManager
{
    private readonly ILLMClientFactory _clientFactory;
    private readonly LLMOptions _llmOptions;

    public LLMClientManager(
        ILLMClientFactory clientFactory,
        IOptions<LLMOptions> llmOptions,
        IModelsStorageService modelsStorageService
    )
    {
        _clientFactory = clientFactory;
        _llmOptions = llmOptions.Value;
        EmbeddingThreshold =
            modelsStorageService.GetEmbeddingModelByName(_llmOptions.EmbeddingsModel)?.Threshold ?? 0.3;
        AIProvider = modelsStorageService.GetAIProviderFromModel(llmOptions.Value.ChatModel, ModelType.ChatModel);
    }

    public string ChatModel => _llmOptions.ChatModel;
    public string EmbeddingModel => _llmOptions.EmbeddingsModel;
    public double EmbeddingThreshold { get; }
    public AIProvider AIProvider { get; }

    public Task<string?> GetCompletionAsync(
        ChatSession chatSession,
        string userQuery,
        string context,
        CancellationToken cancellationToken = default
    )
    {
        var chatItems = PrepareChatItemsCompletion(chatSession, userQuery, context);

        var llmClientStratgey = _clientFactory.CreateClient(AIProvider);

        return llmClientStratgey.GetCompletionAsync(chatItems, cancellationToken);
    }

    public IAsyncEnumerable<string?> GetCompletionStreamAsync(
        ChatSession chatSession,
        string userQuery,
        string context,
        CancellationToken cancellationToken = default
    )
    {
        var chatItems = PrepareChatItemsCompletion(chatSession, userQuery, context);

        var llmClientStratgey = _clientFactory.CreateClient(AIProvider);

        return llmClientStratgey.GetCompletionStreamAsync(chatItems, cancellationToken);
    }

    public Task<IList<double>> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default)
    {
        var llmClientStratgey = _clientFactory.CreateClient(AIProvider);

        return llmClientStratgey.GetEmbeddingAsync(input, cancellationToken);
    }

    private static List<ChatItem> PrepareChatItemsCompletion(ChatSession chatSession, string userQuery, string context)
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
                PromptConstants.CodeAssistantTemplate,
                new { codeContext = context }
            );
            var systemChatItem = chatSession.CreateSystemChatItem(systemCodeAssistPrompt);
            chatItems.Add(systemChatItem);
        }

        var userChatItem = chatSession.CreateUserChatItem(userQuery);
        chatItems.Add(userChatItem);
        return chatItems;
    }
}
