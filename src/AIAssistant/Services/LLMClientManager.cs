using System.Text;
using AIAssistant.Chat.Models;
using AIAssistant.Contracts;
using AIAssistant.Dtos;
using BuildingBlocks.LLM;
using Clients.Contracts;
using Clients.Dtos;
using Clients.Models;
using Clients.Options;
using Microsoft.Extensions.Options;

namespace AIAssistant.Services;

public class LLMClientManager : ILLMClientManager
{
    private readonly ILLMClientFactory _clientFactory;
    private readonly IChatSessionManager _chatSessionManager;
    private readonly ITokenizer _tokenizer;

    public LLMClientManager(
        ILLMClientFactory clientFactory,
        IOptions<LLMOptions> llmOptions,
        IChatSessionManager chatSessionManager,
        ITokenizer tokenizer,
        ICacheModels cacheModels
    )
    {
        _clientFactory = clientFactory;
        _chatSessionManager = chatSessionManager;
        _tokenizer = tokenizer;

        EmbeddingModel = cacheModels.GetModel(llmOptions.Value.EmbeddingsModel);
        ChatModel = cacheModels.GetModel(llmOptions.Value.ChatModel);
        EmbeddingThreshold = EmbeddingModel.ModelOption.Threshold;
    }

    public Model ChatModel { get; }
    public Model EmbeddingModel { get; }
    public decimal EmbeddingThreshold { get; }

    public async IAsyncEnumerable<string?> GetCompletionStreamAsync(
        string userQuery,
        string systemContext,
        CancellationToken cancellationToken = default
    )
    {
        var chatSession = _chatSessionManager.GetCurrentActiveSession();

        chatSession.TrySetSystemContext(systemContext);
        chatSession.AddUserChatItem(userQuery);

        var chatItems = chatSession.GetChatItemsFromHistory();

        var llmClientStratgey = _clientFactory.CreateClient(ChatModel.ModelInformation.AIProvider);

        var chatCompletionResponseStreams = llmClientStratgey.GetCompletionStreamAsync(
            new ChatCompletionRequest(chatItems.Select(x => new ChatCompletionRequestItem(x.Role, x.Prompt))),
            cancellationToken
        );

        StringBuilder chatOutputResponseStringBuilder = new StringBuilder();
        TokenUsageResponse? tokenUsageResponse = null;

        await foreach (var chatCompletionStream in chatCompletionResponseStreams)
        {
            chatOutputResponseStringBuilder.Append(chatCompletionStream?.ChatResponse);

            if (chatCompletionStream?.TokenUsage is not null)
            {
                tokenUsageResponse = chatCompletionStream.TokenUsage;
            }

            yield return chatCompletionStream?.ChatResponse;
        }

        chatSession.AddAssistantChatItem(
            prompt: chatOutputResponseStringBuilder.ToString(),
            inputTokenCount: tokenUsageResponse?.InputTokens
                ?? await _tokenizer.GetTokenCount(string.Concat(chatItems.Select(x => x.Prompt))),
            inputCostPerToken: tokenUsageResponse?.InputCostPerToken ?? 0,
            outputTokenCount: tokenUsageResponse?.OutputTokens
                ?? await _tokenizer.GetTokenCount(chatOutputResponseStringBuilder.ToString()),
            outputCostPerToken: tokenUsageResponse?.OutputCostPerToken ?? 0
        );
    }

    public async Task<GetEmbeddingResult> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default)
    {
        var llmClientStratgey = _clientFactory.CreateClient(EmbeddingModel.ModelInformation.AIProvider);

        var embeddingResponse = await llmClientStratgey.GetEmbeddingAsync(input, cancellationToken);

        // in embedding output tokens and its cost is 0
        var inputTokens = embeddingResponse?.TokenUsage?.InputTokens ?? await _tokenizer.GetTokenCount(input);
        var cost = inputTokens * EmbeddingModel.ModelInformation.InputCostPerToken;

        return new GetEmbeddingResult(embeddingResponse?.Embeddings ?? new List<double>(), inputTokens, cost);
    }
}
