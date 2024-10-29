using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using BuildingBlocks.Serialization;
using Clients.Chat.Models;
using Clients.Contracts;
using Clients.Models;
using Clients.Models.Anthropic;
using Clients.Options;
using Humanizer;
using Microsoft.Extensions.Options;
using Polly.Wrap;

namespace Clients.Anthropic;

// ref: https://docs.anthropic.com/en/api/messages
// https://docs.anthropic.com/en/api/messages-streaming
// https://docs.anthropic.com/en/api/messages-examples

public class AnthropicClient(
    IHttpClientFactory httpClientFactory,
    IOptions<LLMOptions> options,
    AsyncPolicyWrap<HttpResponseMessage> combinedPolicy
) : ILLMClient
{
    private readonly LLMOptions _options = options.Value;

    public async Task<string?> GetCompletionAsync(
        IReadOnlyList<ChatItem> chatItems,
        CancellationToken cancellationToken = default
    )
    {
        var requestBody = new
        {
            model = _options.ChatModel.Trim(),
            messages = chatItems.Select(x => new
            {
                role = x.Role.Humanize(LetterCasing.LowerCase),
                content = x.Prompt,
            }),
            temperature = _options.Temperature,
            max_tokens_to_sample = _options.MaxTokenSize,
        };

        var client = httpClientFactory.CreateClient("llm_client");

        // https://github.com/App-vNext/Polly#handing-return-values-and-policytresult
        var httpResponse = await combinedPolicy.ExecuteAsync(async () =>
        {
            // https://docs.anthropic.com/en/api/complete
            var response = await client.PostAsJsonAsync(
                "v1/complete",
                requestBody,
                cancellationToken: cancellationToken
            );

            return response;
        });

        httpResponse.EnsureSuccessStatusCode();

        var completionResponse = await httpResponse.Content.ReadFromJsonAsync<AnthropicCompletionResponse>(
            options: JsonObjectSerializer.Options,
            cancellationToken: cancellationToken
        );

        return completionResponse?.Content.FirstOrDefault(x => x.Type == "text")?.Text.Trim();
    }

    public async IAsyncEnumerable<string?> GetCompletionStreamAsync(
        IReadOnlyList<ChatItem> chatItems,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var requestBody = new
        {
            model = _options.ChatModel.Trim(),
            messages = chatItems.Select(x => new
            {
                role = x.Role.Humanize(LetterCasing.LowerCase),
                content = x.Prompt,
            }),
            temperature = _options.Temperature,
            max_tokens_to_sample = _options.MaxTokenSize,
            stream = true,
        };

        var client = httpClientFactory.CreateClient("llm_client");

        var httpResponse = await combinedPolicy.ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "v1/complete")
            {
                Content = JsonContent.Create(requestBody),
            };

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            return response;
        });

        httpResponse.EnsureSuccessStatusCode();

        // https://docs.anthropic.com/en/api/messages-streaming
        // Read the response as a stream
        await using var responseStream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var streamReader = new StreamReader(responseStream);

        // Process the response line by line
        while (!streamReader.EndOfStream)
        {
            var line = await streamReader.ReadLineAsync(cancellationToken);

            if (line != null && line.StartsWith("data:"))
            {
                var json = line.Substring("data:".Length).Trim();
                if (!string.IsNullOrEmpty(json))
                {
                    var streamResponse = JsonSerializer.Deserialize<AnthropicStreamedResponse>(
                        json,
                        options: JsonObjectSerializer.Options
                    );

                    if (streamResponse?.Content != null)
                    {
                        var content = streamResponse.Content.FirstOrDefault(x => x.Type == "text_delta")?.Text;
                        if (!string.IsNullOrEmpty(content))
                        {
                            yield return content;
                        }
                    }
                }
            }

            // Check for cancellation
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }
        }
    }

    public Task<IList<double>> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
