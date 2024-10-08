using System.Net.Http.Json;
using BuildingBlocks.Serialization;
using Clients.Anthropic.Models;
using Clients.Contracts;
using Clients.Models;
using Clients.OpenAI.Models.Embeddings;
using Clients.Options;
using Humanizer;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Timeout;
using Polly.Wrap;

namespace Clients.Anthropic;

public class AnthropicClientStratgey : ILLMClientStratgey
{
    private readonly HttpClient _client;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly LLMOptions _options;
    private readonly AsyncPolicyWrap<HttpResponseMessage> _combinedPolicy;

    public AnthropicClientStratgey(
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
        var requestBody = new
        {
            model = _options.ChatModel.Trim(),
            messages = chatItems.Select(x => new
            {
                role = x.Role.Humanize(LetterCasing.LowerCase),
                content = x.Prompt,
            }),
            temperature = 0.2,
            max_tokens_to_sample = _options.MaxTokenSize,
        };

        // https://github.com/App-vNext/Polly#handing-return-values-and-policytresult
        var httpResponse = await _combinedPolicy.ExecuteAsync(async () =>
        {
            // https://docs.anthropic.com/en/api/complete
            var response = await _client.PostAsJsonAsync("v1/complete", requestBody);

            return response;
        });

        httpResponse.EnsureSuccessStatusCode();

        var completionResponse = await httpResponse.Content.ReadFromJsonAsync<AnthropicCompletionResponse>(
            _jsonSerializer.Options
        );

        return completionResponse?.Completion;
    }

    public async Task<IList<double>> GetEmbeddingAsync(string input)
    {
        // https://docs.anthropic.com/en/docs/build-with-claude/embeddings#getting-started-with-voyage-ai
        var requestBody = new { input = new[] { input }, model = _options.EmbeddingsModel.Trim() };

        // https://github.com/App-vNext/Polly#handing-return-values-and-policytresult
        var httpResponse = await _combinedPolicy.ExecuteAsync(async () =>
        {
            // https://docs.anthropic.com/en/docs/build-with-claude/embeddings#voyage-http-api
            var response = await _client.PostAsJsonAsync("v1/embeddings", requestBody); // anthropic doesn't have its own embedding, and we can use Voyage model

            return response;
        });

        httpResponse.EnsureSuccessStatusCode();

        var embeddingResponse = await httpResponse.Content.ReadFromJsonAsync<OpenAiEmbeddingResponse>(
            _jsonSerializer.Options
        );

        return embeddingResponse?.Data.FirstOrDefault()?.Embedding ?? new List<double>();
    }
}
