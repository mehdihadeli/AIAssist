using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Clients.Contracts;
using Clients.Dtos;

namespace Clients;

public class GeminiClient : IChatClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;

    public GeminiClient(IHttpClientFactory httpClientFactory, string apiKey)
    {
        _httpClientFactory = httpClientFactory;
        _apiKey = apiKey;
    }

    public async IAsyncEnumerable<ChatCompletionResponse?> GetCompletionStreamAsync(
        ChatCompletionRequest chatCompletionRequest,
        CancellationToken cancellationToken = default
    )
    {
        var client = _httpClientFactory.CreateClient("gemini_client");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        var requestBody = new
        {
            inputs = chatCompletionRequest.Items.Select(x => new
            {
                role = x.Role.ToString().ToLower(CultureInfo.CurrentCulture),
                message = x.Prompt
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
            var jsonData = JsonSerializer.Deserialize<GeminiStreamResponse>(line);
            if (jsonData != null)
            {
                yield return new ChatCompletionResponse(
                    jsonData.Content,
                    null // Gemini API may not provide per-message token usage in stream mode.
                );
            }
        }
    }
}

// Gemini-specific DTOs for parsing streaming API responses.
public record GeminiStreamResponse(string? Content);