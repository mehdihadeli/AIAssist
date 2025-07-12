using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Clients.Contracts;
using Clients.Dtos;

namespace Clients;

public class QwenClient : IChatClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;

    public QwenClient(IHttpClientFactory httpClientFactory, string apiKey)
    {
        _httpClientFactory = httpClientFactory;
        _apiKey = apiKey;
    }

    public async IAsyncEnumerable<ChatCompletionResponse?> GetCompletionStreamAsync(
        ChatCompletionRequest chatCompletionRequest,
        CancellationToken cancellationToken = default
    )
    {
        var client = _httpClientFactory.CreateClient("qwen_client");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        var requestBody = new
        {
            messages = chatCompletionRequest.Items.Select(x => new
            {
                role = x.Role.ToString().ToLower(CultureInfo.CurrentCulture),
                content = x.Prompt
            }),
            stream = true
        };

        var response = await client.PostAsJsonAsync("/v1/stream", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var streamReader = new StreamReader(responseStream);

        while (!streamReader.EndOfStream)
        {
            var line = await streamReader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line)) continue;

            var jsonData = JsonSerializer.Deserialize<QwenStreamResponse>(line);
            if (jsonData != null)
            {
                yield return new ChatCompletionResponse(
                    jsonData.Message,
                    jsonData.Usage is not null
                        ? new TokenUsageResponse(
                            jsonData.Usage.InputTokens,
                            jsonData.Usage.InputCostPerToken,
                            jsonData.Usage.OutputTokens,
                            jsonData.Usage.OutputCostPerToken
                        )
                        : null
                );
            }
        }
    }
}

// Qwen-specific DTOs for response parsing.
public record QwenStreamResponse(string? Message, QwenTokenUsage? Usage);
public record QwenTokenUsage(int InputTokens, decimal InputCostPerToken, int OutputTokens, decimal OutputCostPerToken);