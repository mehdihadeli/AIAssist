using System.Net.Http.Json;
using BuildingBlocks.Serialization;
using Clients.Contracts;
using Clients.Models;
using Clients.Ollama.Models.Completion;
using Clients.Ollama.Models.Embeddings;
using Clients.Options;
using Humanizer;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Timeout;
using Polly.Wrap;

namespace Clients.Ollama;

public class OllamaClientStratgey : ILLMClientStratgey
{
    private readonly HttpClient _client;
    private readonly LLMOptions _options;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly AsyncPolicyWrap<HttpResponseMessage> _combinedPolicy;

    public OllamaClientStratgey(
        HttpClient client,
        IOptions<LLMOptions> options,
        IJsonSerializer jsonSerializer,
        IOptions<PolicyOptions> policyOptions
    )
    {
        _client = client;
        _options = options.Value;
        _jsonSerializer = jsonSerializer;

        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .RetryAsync(policyOptions.Value.RetryCount);

        // HttpClient itself will still enforce its own timeout, which is 100 seconds by default. To fix this issue, you need to set the HttpClient.Timeout property to match or exceed the timeout configured in Polly's policy.
        var timeoutPolicy = Policy.TimeoutAsync(policyOptions.Value.TimeoutSeconds, TimeoutStrategy.Pessimistic);

        // at any given time there will 3 parallel requests execution for specific service call and another 6 requests for other services can be in the queue. So that if the response from customer service is delayed or blocked then we don’t use too many resources
        var bulkheadPolicy = Policy.BulkheadAsync<HttpResponseMessage>(3, 6);

        // https://github.com/App-vNext/Polly#handing-return-values-and-policytresult
        var circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                policyOptions.Value.RetryCount + 1,
                TimeSpan.FromSeconds(policyOptions.Value.BreakDuration)
            );

        var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, bulkheadPolicy);

        _combinedPolicy = combinedPolicy.WrapAsync(timeoutPolicy);
    }

    public async Task<string?> GetCompletionAsync(IReadOnlyList<ChatItem> chatItems)
    {
        // https://platform.openai.com/docs/guides/text-generation/building-prompts
        var requestBody = new
        {
            model = _options.ChatModel.Trim(),
            messages = chatItems.Select(x => new
            {
                role = x.Role.Humanize(LetterCasing.LowerCase),
                content = x.Prompt,
            }),
            temperature = 0.2,
            max_tokens = _options.MaxTokenSize,
        };

        // https://github.com/App-vNext/Polly#handing-return-values-and-policytresult
        var httpResponse = await _combinedPolicy.ExecuteAsync(async () =>
        {
            // https://ollama.com/blog/openai-compatibility
            // https://www.youtube.com/watch?v=38jlvmBdBrU
            // https://platform.openai.com/docs/api-reference/chat/create
            // https://github.com/ollama/ollama/blob/main/docs/api.md#generate-a-chat-completion
            var response = await _client.PostAsJsonAsync("v1/chat/completions", requestBody);

            return response;
        });

        httpResponse.EnsureSuccessStatusCode();

        var completionResponse = await httpResponse.Content.ReadFromJsonAsync<LlamaCompletionResponse>(
            _jsonSerializer.Options
        );

        return completionResponse
            ?.Choices.FirstOrDefault(x => x.Message.Role == RoleType.Assistant)
            ?.Message.Content.Trim();
    }

    public async Task<IList<double>> GetEmbeddingAsync(string input)
    {
        var requestBody = new
        {
            input = new[] { input },
            model = _options.EmbeddingsModel.Trim(),
            encoding_format = "float",
        };

        // https://github.com/App-vNext/Polly#handing-return-values-and-policytresult
        var httpResponse = await _combinedPolicy.ExecuteAsync(async () =>
        {
            // https://github.com/ollama/ollama/blob/main/docs/api.md#generate-embeddings
            // https://platform.openai.com/docs/api-reference/embeddings
            // https://ollama.com/blog/embedding-models
            var response = await _client.PostAsJsonAsync("v1/embeddings", requestBody);

            return response;
        });

        httpResponse.EnsureSuccessStatusCode();

        var embeddingResponse = await httpResponse.Content.ReadFromJsonAsync<LlamaEmbeddingResponse>(
            _jsonSerializer.Options
        );

        return embeddingResponse?.Data.FirstOrDefault()?.Embedding ?? new List<double>();
    }
}
