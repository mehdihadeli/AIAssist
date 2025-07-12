using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Clients.Contracts;
using Clients.Dtos;

namespace Clients;

public class GrokClient : IChatClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;

    public GrokClient(IHttpClientFactory httpClientFactory, string apiKey)
    {
        _httpClientFactory = httpClientFactory;
        _apiKey = apiKey;
    }

    public async IAsyncEnumerable<ChatCompletionResponse?> GetCompletionStreamAsync(
        ChatCompletionRequest chatCompletionRequest,
        CancellationToken cancellationToken = default
    )
    {
        var client = _httpClientFactory.CreateClient("grok_client");
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

            // Parse the JSON data per chunk/line.
            var jsonData = JsonSerializer.Deserialize<GrokStreamResponse>(line);
            if (jsonData != null)
            {
                yield return new ChatCompletionResponse(
                    jsonData.Content,
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

// Grok-specific DTOs for response parsing.
public record GrokStreamResponse(string? Content, GrokTokenUsage? Usage);
public record GrokTokenUsage(int InputTokens, decimal InputCostPerToken, int OutputTokens, decimal OutputCostPerToken);