using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Clients.Contracts;
using Clients.Dtos;

namespace Clients;

public class OpenRouterClient : IChatClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;
    private readonly string _model;

    public OpenRouterClient(IHttpClientFactory httpClientFactory, string apiKey, string model = "gpt-4")
    {
        _httpClientFactory = httpClientFactory;
        _apiKey = apiKey;
        _model = model;
    }

    public async IAsyncEnumerable<ChatCompletionResponse?> GetCompletionStreamAsync(
        ChatCompletionRequest chatCompletionRequest,
        CancellationToken cancellationToken = default
    )
    {
        var client = _httpClientFactory.CreateClient("openrouter_client");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        var requestBody = new
        {
            model = _model,
            messages = chatCompletionRequest.Items.Select(x => new
            {
                role = x.Role.ToString().ToLower(CultureInfo.CurrentCulture),
                content = x.Prompt
            }),
            stream = true
        };

        var response = await client.PostAsJsonAsync("v1/completions", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var streamReader = new StreamReader(responseStream);

        while (!streamReader.EndOfStream)
        {
            var line = await streamReader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.StartsWith("data: [DONE]")) break;

            var jsonData = line.StartsWith("data: ") ? line[6..] : line;
            var streamResponse = JsonSerializer.Deserialize<OpenRouterResponse>(jsonData);
            var message = streamResponse?.Choices?.FirstOrDefault()?.Message;

            if (message is not null)
            {
                yield return new ChatCompletionResponse(
                    message,
                    null
                );
            }
        }
    }
}

// OpenRouter-specific DTOs for response parsing.
public record OpenRouterResponse(IEnumerable<OpenRouterChoice>? Choices, OpenRouterUsage? Usage);
public record OpenRouterChoice(string? Message);
public record OpenRouterUsage(int InputTokens, decimal InputCostPerToken, int OutputTokens, decimal OutputCostPerToken);