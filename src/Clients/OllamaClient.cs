using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using BuildingBlocks.Serialization;
using Clients.Chat.Models;
using Clients.Contracts;
using Clients.Models;
using Clients.Models.Ollama.Completion;
using Clients.Models.Ollama.Embeddings;
using Clients.Models.OpenAI.Completion;
using Clients.Options;
using Humanizer;
using Microsoft.Extensions.Options;
using Polly.Wrap;
using Spectre.Console;

namespace Clients.Ollama;

public class OllamaClient(
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
        // https://platform.openai.com/docs/guides/text-generation/building-prompts
        var requestBody = new
        {
            model = _options.ChatModel,
            messages = chatItems.Select(x => new
            {
                role = x.Role.Humanize(LetterCasing.LowerCase),
                content = x.Prompt,
            }),
            // Temperature set to 0 to reduce the randomness of the response. Better for applications that expect consistent responses.
            temperature = _options.Temperature,
            max_tokens = _options.MaxTokenSize,
        };

        var client = httpClientFactory.CreateClient("llm_client");

        // https://github.com/App-vNext/Polly#handing-return-values-and-policytresult
        var httpResponse = await combinedPolicy.ExecuteAsync(async () =>
        {
            // https://ollama.com/blog/openai-compatibility
            // https://www.youtube.com/watch?v=38jlvmBdBrU
            // https://platform.openai.com/docs/api-reference/chat/create
            // https://github.com/ollama/ollama/blob/main/docs/api.md#generate-a-chat-completion
            var response = await client.PostAsJsonAsync(
                "v1/chat/completions",
                requestBody,
                cancellationToken: cancellationToken
            );

            return response;
        });

        httpResponse.EnsureSuccessStatusCode();

        var completionResponse = await httpResponse.Content.ReadFromJsonAsync<LlamaCompletionResponse>(
            options: JsonObjectSerializer.Options,
            cancellationToken: cancellationToken
        );

        return completionResponse
            ?.Choices.FirstOrDefault(x => x.Message.Role == RoleType.Assistant)
            ?.Message.Content.Trim();
    }

    public async IAsyncEnumerable<string?> GetCompletionStreamAsync(
        IReadOnlyList<ChatItem> chatItems,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        AnsiConsole.Write(
            new Text(
                chatItems.SingleOrDefault(x => x.Role == RoleType.User)?.Prompt,
                new Style(foreground: Color.Green)
            )
        );
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule());
        AnsiConsole.Write(
            new Text(
                chatItems.SingleOrDefault(x => x.Role == RoleType.System).Prompt,
                new Style(foreground: Color.LightSkyBlue3_1)
            )
        );
        AnsiConsole.Write(new Rule());

        var requestBody = new
        {
            model = _options.ChatModel,
            messages = chatItems.Select(x => new
            {
                role = x.Role.Humanize(LetterCasing.LowerCase),
                content = x.Prompt,
            }),
            temperature = _options.Temperature,
            max_tokens = _options.MaxTokenSize,
            stream = true,
        };

        var client = httpClientFactory.CreateClient("llm_client");

        // Execute the policy with streaming support
        var httpResponse = await combinedPolicy.ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
            {
                Content = JsonContent.Create(requestBody),
            };

            // Send the request and get the response
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            return response;
        });

        // Ensure the response is successful
        httpResponse.EnsureSuccessStatusCode();

        // https://platform.openai.com/docs/api-reference/chat/create#chat-create-stream
        // https://cookbook.openai.com/examples/how_to_stream_completions
        // Read the response stream
        await using var responseStream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var streamReader = new StreamReader(responseStream);

        while (!streamReader.EndOfStream)
        {
            var line = await streamReader.ReadLineAsync(cancellationToken);

            if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
                continue;

            // when we reached to the end of the streams
            if (line.StartsWith("data: [DONE]"))
                break;

            // Parse the streaming data (assume JSON format)
            if (line.StartsWith("data: "))
            {
                var jsonData = line.Substring("data: ".Length);
                if (string.IsNullOrEmpty(jsonData))
                    continue;

                var streamResponse = JsonSerializer.Deserialize<OpenAICompletionStreamResponse>(
                    jsonData,
                    options: JsonObjectSerializer.Options
                );

                if (streamResponse?.Choices != null)
                {
                    var content = streamResponse
                        .Choices.FirstOrDefault(x => x.Delta.Role == RoleType.Assistant)
                        ?.Delta.Content;

                    // Yield the content to the async stream
                    yield return content;
                }
            }
        }
    }

    public async Task<IList<double>> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default)
    {
        var requestBody = new { input = new[] { input }, model = _options.EmbeddingsModel };

        var client = httpClientFactory.CreateClient("llm_client");

        // https://github.com/App-vNext/Polly#handing-return-values-and-policytresult
        var httpResponse = await combinedPolicy.ExecuteAsync(async () =>
        {
            // https://github.com/ollama/ollama/blob/main/docs/api.md#generate-embeddings
            // https://platform.openai.com/docs/api-reference/embeddings
            // https://ollama.com/blog/embedding-models
            var response = await client.PostAsJsonAsync(
                "v1/embeddings",
                requestBody,
                cancellationToken: cancellationToken
            );

            return response;
        });

        httpResponse.EnsureSuccessStatusCode();

        var embeddingResponse = await httpResponse.Content.ReadFromJsonAsync<LlamaEmbeddingResponse>(
            options: JsonObjectSerializer.Options,
            cancellationToken: cancellationToken
        );

        return embeddingResponse?.Data.FirstOrDefault()?.Embedding ?? new List<double>();
    }
}
