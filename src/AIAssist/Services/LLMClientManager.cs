using System.Text;
using AIAssist.Contracts;
using AIAssist.Dtos;
using BuildingBlocks.LLM;
using Clients.Contracts;
using Clients.Dtos;
using Clients.Models;
using Clients.Options;
using Microsoft.Extensions.Options;

namespace AIAssist.Services;

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
        ChatModel =
            cacheModels.GetModel(llmOptions.Value.ChatModel)
            ?? throw new ArgumentNullException($"Model '{llmOptions.Value.ChatModel}' not found in the CacheModels.");
        EmbeddingThreshold = EmbeddingModel?.Threshold ?? 0.2m;
    }

    public Model ChatModel { get; }
    public Model? EmbeddingModel { get; }
    public decimal EmbeddingThreshold { get; }

    public async IAsyncEnumerable<string?> GetCompletionStreamAsync(
        string userQuery,
        string? systemPrompt,
        CancellationToken cancellationToken = default
    )
    {
        var chatSession = _chatSessionManager.GetCurrentActiveSession();

        chatSession.TrySetSystemContext(systemPrompt);
        chatSession.AddUserChatItem(userQuery);

        var chatItems = chatSession.GetChatItemsFromHistory();

        var llmClientStratgey = _clientFactory.CreateClient(ChatModel.AIProvider);

        var chatCompletionResponseStreams = llmClientStratgey.GetCompletionStreamAsync(
            new ChatCompletionRequest(chatItems.Select(x => new ChatCompletionRequestItem(x.Role, x.Prompt))),
            cancellationToken
        );

        StringBuilder chatOutputResponseStringBuilder = new StringBuilder();
        TokenUsageResponse? tokenUsageResponse = null;

        await foreach (var chatCompletionStream in chatCompletionResponseStreams)
        {
            if (chatCompletionStream?.ChatResponse is null)
            {
                continue;
            }

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

    public async Task<GetEmbeddingResult> GetEmbeddingAsync(
        IList<string> inputs,
        string? path,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(EmbeddingModel);
        var llmClientStratgey = _clientFactory.CreateClient(EmbeddingModel.AIProvider);

        var embeddingResponse = await llmClientStratgey.GetEmbeddingAsync(inputs, path, cancellationToken);

        // in embedding output tokens and its cost is 0
        var inputTokens =
            embeddingResponse?.TokenUsage?.InputTokens ?? await _tokenizer.GetTokenCount(string.Concat(inputs));
        var cost = inputTokens * EmbeddingModel.InputCostPerToken;

        return new GetEmbeddingResult(embeddingResponse?.Embeddings ?? new List<IList<double>>(), inputTokens, cost);
    }
}
