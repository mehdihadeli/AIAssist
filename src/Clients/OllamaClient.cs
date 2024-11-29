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

// Ref: https://github.com/ollama/ollama/blob/main/docs/api.md

public class OllamaClient(
    IHttpClientFactory httpClientFactory,
    IOptions<LLMOptions> llmOptions,
    ICacheModels cacheModels,
    ITokenizer tokenizer,
    AsyncPolicyWrap<HttpResponseMessage> combinedPolicy
) : ILLMClient
{
    private readonly Model _chatModel =
        cacheModels.GetModel(llmOptions.Value.ChatModel)
        ?? throw new KeyNotFoundException($"Model '{llmOptions.Value.ChatModel}' not found in the ModelCache.");
    private readonly Model? _embeddingModel = cacheModels.GetModel(llmOptions.Value.EmbeddingsModel);
    private const int MaxRequestSizeInBytes = 100 * 1024; // 100KB

    public async Task<ChatCompletionResponse?> GetCompletionAsync(
        ChatCompletionRequest chatCompletionRequest,
        CancellationToken cancellationToken = default
    )
    {
        await ValidateChatMaxInputToken(chatCompletionRequest);
        ValidateRequestSizeAndContent(chatCompletionRequest);

        // https://github.com/ollama/ollama/blob/main/docs/api.md#generate-a-chat-completion
        var requestBody = new
        {
            model = _chatModel.Name,
            messages = chatCompletionRequest.Items.Select(x => new
            {
                role = x.Role.Humanize(LetterCasing.LowerCase),
                content = x.Prompt,
            }),
            options = new { temperature = _chatModel.Temperature },
            keep_alive = "30m",
            stream = false,
        };

        var client = httpClientFactory.CreateClient("llm_chat_client");

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
        var inputCostPerToken = _chatModel.InputCostPerToken;
        var outputCostPerToken = _chatModel.OutputCostPerToken;

        ValidateChatMaxToken(inputTokens + outTokens);

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
        await ValidateChatMaxInputToken(chatCompletionRequest);
        ValidateRequestSizeAndContent(chatCompletionRequest);

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
            options = new { temperature = _chatModel.Temperature },
            stream = true,
            keep_alive = "30m",
        };

        var client = httpClientFactory.CreateClient("llm_chat_client");

        // Execute the policy with streaming support
        var httpResponseMessage = await combinedPolicy.ExecuteAsync(async () =>
        {
            var response = await client.PostAsJsonAsync("api/chat", requestBody, cancellationToken: cancellationToken);

            return response;
        });

        if (httpResponseMessage.IsSuccessStatusCode)
        {
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

                var completionStreamResponse = JsonSerializer.Deserialize<LlamaCompletionResponse>(
                    jsonData,
                    options: JsonObjectSerializer.SnakeCaseOptions
                );

                if (completionStreamResponse is null)
                    continue;

                var completionMessage = completionStreamResponse.Message.Content;

                if (completionMessage is null)
                    continue;

                if (completionStreamResponse.Done)
                {
                    // https://github.com/ollama/ollama/blob/main/docs/api.md#response-9
                    var inputTokens = completionStreamResponse.PromptEvalCount;
                    var outTokens = completionStreamResponse.EvalCount;
                    var inputCostPerToken = _chatModel.InputCostPerToken;
                    var outputCostPerToken = _chatModel.OutputCostPerToken;

                    ValidateChatMaxToken(inputTokens + outTokens);

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
        else
        {
            var completionResponse = await httpResponseMessage.Content.ReadFromJsonAsync<LlamaCompletionResponse>(
                cancellationToken: cancellationToken
            );
            HandleException(httpResponseMessage, completionResponse);
        }
    }

    public async Task<EmbeddingsResponse?> GetEmbeddingAsync(
        IList<string> inputs,
        string? path,
        CancellationToken cancellationToken = default
    )
    {
        await ValidateEmbeddingMaxInputToken(string.Concat(inputs), path);
        ValidateRequestSizeAndContent(string.Concat(inputs));

        ArgumentNullException.ThrowIfNull(_embeddingModel);

        // https://github.com/ollama/ollama/blob/main/docs/api.md#generate-embeddings
        var requestBody = new
        {
            input = inputs,
            model = _embeddingModel.Name,
            options = new { temperature = _embeddingModel.Temperature },
            keep_alive = "30m",
        };

        var client = httpClientFactory.CreateClient("llm_embeddings_client");

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

        var inputTokens = embeddingResponse.PromptEvalCount;
        var outTokens = embeddingResponse.EvalCount;
        var inputCostPerToken = _embeddingModel.InputCostPerToken;
        var outputCostPerToken = _embeddingModel.OutputCostPerToken;

        ValidateEmbeddingMaxToken(inputTokens + outTokens, path);

        var embedding = embeddingResponse.Embeddings ?? new List<IList<double>>();

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

    private async Task ValidateChatMaxInputToken(ChatCompletionRequest chatCompletionRequest)
    {
        var inputTokenCount = await tokenizer.GetTokenCount(
            string.Concat(chatCompletionRequest.Items.Select(x => x.Prompt))
        );

        if (_chatModel.MaxInputTokens > 0 && inputTokenCount > _chatModel.MaxInputTokens)
        {
            throw new OllamaException(
                $"current chat 'max_input_token' count: {inputTokenCount.FormatCommas()} is larger than configured 'max_input_token' count: {_chatModel.MaxInputTokens.FormatCommas()}.",
                HttpStatusCode.BadRequest
            );
        }
    }

    private async Task ValidateEmbeddingMaxInputToken(string input, string? path = null)
    {
        var inputTokenCount = await tokenizer.GetTokenCount(input);

        ArgumentNullException.ThrowIfNull(_embeddingModel);
        if (_embeddingModel.MaxInputTokens > 0 && inputTokenCount > _embeddingModel.MaxInputTokens)
        {
            var moreInfo = path is not null
                ? $"if file '{
                        path
                    }' is not required for embedding you can ignore that by adding file or folder to '.aiassistignore'"
                : "";

            throw new OllamaException(
                $"embedding {path} 'max_input_token' count: {inputTokenCount.FormatCommas()} is larger than configured 'max_input_token' count: {_embeddingModel.MaxInputTokens.FormatCommas()}. {moreInfo}",
                HttpStatusCode.BadRequest
            );
        }
    }

    private void ValidateChatMaxToken(int maxTokenCount)
    {
        if (_chatModel.MaxTokens > 0 && maxTokenCount > _chatModel.MaxTokens)
        {
            throw new OllamaException(
                $"current chat 'max_token' count: {maxTokenCount.FormatCommas()} is larger than configured 'max_token' count: {_chatModel.MaxTokens.FormatCommas()}.",
                HttpStatusCode.BadRequest
            );
        }
    }

    private void ValidateEmbeddingMaxToken(int maxTokenCount, string? path)
    {
        ArgumentNullException.ThrowIfNull(_embeddingModel);
        if (_embeddingModel.MaxTokens > 0 && maxTokenCount > _embeddingModel.MaxTokens)
        {
            throw new OllamaException(
                $"embedding {path} 'max_token' count: {maxTokenCount.FormatCommas()} is larger than configured 'max_token' count: {_embeddingModel.MaxTokens.FormatCommas()}.",
                HttpStatusCode.BadRequest
            );
        }
    }

    private void ValidateRequestSizeAndContent(ChatCompletionRequest chatCompletionRequest)
    {
        ValidateRequestSizeAndContent(string.Concat(chatCompletionRequest.Items.Select(x => x.Prompt)));
    }

    private void ValidateRequestSizeAndContent(string input)
    {
        var requestBodySizeInBytes = System.Text.Encoding.UTF8.GetByteCount(input);

        if (requestBodySizeInBytes > MaxRequestSizeInBytes)
        {
            throw new OllamaException(
                $"Request size {requestBodySizeInBytes} bytes exceeds the 100KB limit.",
                HttpStatusCode.BadRequest
            );
        }
    }
}
