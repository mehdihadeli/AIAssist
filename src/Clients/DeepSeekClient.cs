using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Clients.Contracts;
using Clients.Dtos;

namespace Clients;

public class DeepSeekClient : IChatClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;

    public DeepSeekClient(IHttpClientFactory httpClientFactory, string apiKey)
    {
        _httpClientFactory = httpClientFactory;
        _apiKey = apiKey;
    }

    public async IAsyncEnumerable<ChatCompletionResponse?> GetCompletionStreamAsync(
        ChatCompletionRequest chatCompletionRequest,
        CancellationToken cancellationToken = default
    )
    {
        var client = _httpClientFactory.CreateClient("deepseek_client");
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

        var response = await client.PostAsJsonAsync("api/chat/completions/stream", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var streamReader = new StreamReader(responseStream);

        while (!streamReader.EndOfStream)
        {
            var line = await streamReader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line)) continue;

            var jsonData = JsonSerializer.Deserialize<DeepSeekResponse>(line);
            if (jsonData != null)
            {
                yield return new ChatCompletionResponse(
                    jsonData.Content, 
                    null
                );
            }
        }
    }
}

// DeepSeek-specific DTOs for response parsing.
public record DeepSeekResponse(string? Content, DeepSeekUsage? Usage);
public record DeepSeekUsage(int InputTokens, decimal InputCostPerToken, int OutputTokens, decimal OutputCostPerToken);