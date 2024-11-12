using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using BuildingBlocks.LLM;
using BuildingBlocks.Serialization;
using BuildingBlocks.Utils;
using Clients.Contracts;
using Clients.Dtos;
using Clients.Models;
using Clients.Models.Ollama;
using Clients.Models.Ollama.Completion;
using Clients.Models.Ollama.Embeddings;
using Clients.Options;
using Humanizer;
using Microsoft.Extensions.Options;
using Polly.Wrap;
using Spectre.Console;

namespace Clients;

public class OllamaClient(
    IHttpClientFactory httpClientFactory,
    IOptions<LLMOptions> options,
    ICacheModels cacheModels,
    ITokenizer tokenizer,
    AsyncPolicyWrap<HttpResponseMessage> combinedPolicy
) : ILLMClient
{
    private readonly Model _chatModel = cacheModels.GetModel(options.Value.ChatModel);
    private readonly Model _embeddingModel = cacheModels.GetModel(options.Value.EmbeddingsModel);

    public async Task<ChatCompletionResponse?> GetCompletionAsync(
        ChatCompletionRequest chatCompletionRequest,
        CancellationToken cancellationToken = default
    )
    {
        await ValidateMaxInputToken(chatCompletionRequest);

        // https://github.com/ollama/ollama/blob/main/docs/api.md#generate-a-chat-completion
        var requestBody = new
        {
            model = _chatModel.Name,
            messages = chatCompletionRequest.Items.Select(x => new
            {
                role = x.Role.Humanize(LetterCasing.LowerCase),
                content = x.Prompt,
            }),
            options = new { temperature = _chatModel.ModelOption.Temperature },
            keep_alive = "30m",
            stream = false,
        };

        var client = httpClientFactory.CreateClient("llm_client");

        // https://github.com/App-vNext/Polly#handing-return-values-and-policytresult
        var httpResponseMessage = await combinedPolicy.ExecuteAsync(async () =>
        {
            var response = await client.PostAsJsonAsync("api/chat", requestBody, cancellationToken: cancellationToken);

            return response;
        });

        var completionResponse = await httpResponseMessage.Content.ReadFromJsonAsync<LlamaCompletionResponse>(
            options: JsonObjectSerializer.SnakeCaseOptions,
            cancellationToken: cancellationToken
        );

        HandleException(httpResponseMessage, completionResponse);

        var completionMessage = completionResponse.Message.Content;

        var inputTokens = completionResponse.PromptEvalCount;
        var outTokens = completionResponse.EvalCount;
        var inputCostPerToken = _chatModel.ModelInformation.InputCostPerToken;
        var outputCostPerToken = _chatModel.ModelInformation.OutputCostPerToken;

        ValidateMaxToken(inputTokens + outTokens);

        return new ChatCompletionResponse(
            completionMessage,
            new TokenUsageResponse(inputTokens, inputCostPerToken, outTokens, outputCostPerToken)
        );
    }

    public async IAsyncEnumerable<ChatCompletionResponse?> GetCompletionStreamAsync(
        ChatCompletionRequest chatCompletionRequest,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await ValidateMaxInputToken(chatCompletionRequest);

        foreach (var item in chatCompletionRequest.Items)
        {
            AnsiConsole.WriteLine(item.Prompt);
            AnsiConsole.WriteLine("------------");
        }

        // https://github.com/ollama/ollama/blob/main/docs/api.md#generate-a-chat-completion
        // https://github.com/ollama/ollama/pull/6784
        // for now doesn't support `include_usage` for open ai compatibility apis
        var requestBody = new
        {
            model = _chatModel.Name,
            messages = chatCompletionRequest.Items.Select(x => new
            {
                role = x.Role.Humanize(LetterCasing.LowerCase),
                content = x.Prompt,
            }),
            options = new { temperature = _chatModel.ModelOption.Temperature },
            stream = true,
            keep_alive = "30m",
        };

        var client = httpClientFactory.CreateClient("llm_client");

        // Execute the policy with streaming support
        var httpResponseMessage = await combinedPolicy.ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/chat")
            {
                Content = JsonContent.Create(requestBody),
            };

            // Send the request and get the response
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            return response;
        });

        // https://platform.openai.com/docs/api-reference/chat/create#chat-create-stream
        // https://cookbook.openai.com/examples/how_to_stream_completions
        // Read the response stream
        await using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

        using var streamReader = new StreamReader(responseStream);

        while (!streamReader.EndOfStream)
        {
            var jsonData = await streamReader.ReadLineAsync(cancellationToken);

            if (string.IsNullOrEmpty(jsonData) || string.IsNullOrWhiteSpace(jsonData))
                continue;

            var completionResponse = JsonSerializer.Deserialize<LlamaCompletionResponse>(
                jsonData,
                options: JsonObjectSerializer.SnakeCaseOptions
            );

            HandleException(httpResponseMessage, completionResponse);

            var completionMessage = completionResponse.Message.Content;

            if (completionResponse.Done)
            {
                // https://github.com/ollama/ollama/blob/main/docs/api.md#response-9
                var inputTokens = completionResponse.PromptEvalCount;
                var outTokens = completionResponse.EvalCount;
                var inputCostPerToken = _chatModel.ModelInformation.InputCostPerToken;
                var outputCostPerToken = _chatModel.ModelInformation.OutputCostPerToken;

                ValidateMaxToken(inputTokens + outTokens);

                yield return new ChatCompletionResponse(
                    completionMessage,
                    new TokenUsageResponse(inputTokens, inputCostPerToken, outTokens, outputCostPerToken)
                );
            }
            else
            {
                yield return new ChatCompletionResponse(completionMessage, null);
            }
        }
    }

    public async Task<EmbeddingsResponse?> GetEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default
    )
    {
        await ValidateMaxInputToken(input);

        // https://github.com/ollama/ollama/blob/main/docs/api.md#generate-embeddings
        var requestBody = new
        {
            input = new[] { input },
            model = _embeddingModel.Name,
            options = new { temperature = _embeddingModel.ModelOption.Temperature },
            keep_alive = "30m",
        };

        var client = httpClientFactory.CreateClient("llm_client");

        // https://github.com/App-vNext/Polly#handing-return-values-and-policytresult
        var httpResponseMessage = await combinedPolicy.ExecuteAsync(async () =>
        {
            // https://github.com/ollama/ollama/blob/main/docs/api.md#generate-embeddings
            // https://platform.openai.com/docs/api-reference/embeddings
            // https://ollama.com/blog/embedding-models
            var response = await client.PostAsJsonAsync("api/embed", requestBody, cancellationToken: cancellationToken);

            return response;
        });

        var embeddingResponse = await httpResponseMessage.Content.ReadFromJsonAsync<LlamaEmbeddingResponse>(
            options: JsonObjectSerializer.SnakeCaseOptions,
            cancellationToken: cancellationToken
        );

        HandleException(httpResponseMessage, embeddingResponse);

        var embedding = embeddingResponse.Embeddings.FirstOrDefault() ?? new List<double>();

        var inputTokens = embeddingResponse.PromptEvalCount;
        var outTokens = embeddingResponse.EvalCount;
        var inputCostPerToken = _embeddingModel.ModelInformation.InputCostPerToken;
        var outputCostPerToken = _embeddingModel.ModelInformation.OutputCostPerToken;

        ValidateMaxToken(inputTokens + outTokens);

        return new EmbeddingsResponse(
            embedding,
            new TokenUsageResponse(inputTokens, inputCostPerToken, outTokens, outputCostPerToken)
        );
    }

    private void HandleException(HttpResponseMessage httpResponse, [NotNull] OllamaResponseBase? opneaiBaseResponse)
    {
        if (opneaiBaseResponse is null)
        {
            httpResponse.EnsureSuccessStatusCode();
        }

        if (!httpResponse.IsSuccessStatusCode && string.IsNullOrEmpty(opneaiBaseResponse!.Error))
        {
            opneaiBaseResponse.Error = httpResponse.ReasonPhrase ?? httpResponse.StatusCode.ToString();
        }

        if (opneaiBaseResponse!.Error is not null)
        {
            throw new OllamaException(opneaiBaseResponse.Error, httpResponse.StatusCode);
        }
    }

    private Task ValidateMaxInputToken(ChatCompletionRequest chatCompletionRequest)
    {
        return ValidateMaxInputToken(string.Concat(chatCompletionRequest.Items.Select(x => x.Prompt)));
    }

    private async Task ValidateMaxInputToken(string input)
    {
        var inputTokenCount = await tokenizer.GetTokenCount(input);

        if (
            _chatModel.ModelInformation.MaxInputTokens > 0
            && inputTokenCount > _chatModel.ModelInformation.MaxInputTokens
        )
        {
            throw new OllamaException(
                $"'max_input_token' count: {inputTokenCount.FormatCommas()} is larger than configured 'max_input_token' count: {_chatModel.ModelInformation.MaxInputTokens.FormatCommas()}, if you need more token change the configuration.",
                HttpStatusCode.BadRequest
            );
        }
    }

    private void ValidateMaxToken(int maxTokenCount)
    {
        if (_chatModel.ModelInformation.MaxTokens > 0 && maxTokenCount > _chatModel.ModelInformation.MaxTokens)
        {
            throw new OllamaException(
                $"'max_token' count: {maxTokenCount.FormatCommas()} is larger than configured 'max_token' count: {_chatModel.ModelInformation.MaxTokens.FormatCommas()}, if you need more token change the configuration.",
                HttpStatusCode.BadRequest
            );
        }
    }
}
