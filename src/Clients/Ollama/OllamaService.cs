using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.LLM;
using Clients.Models;
using Clients.Ollama.Models;
using Clients.Prompts;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Clients.Ollama;

public class OllamaService(HttpClient client, IOptions<OllamaOptions> options, ILogger<OllamaService> logger)
    : ILanguageModelService
{
    private readonly OllamaOptions _options = options.Value;

    public async Task<string> GetCompletionAsync(string userQuery, string codeContext)
    {
        var systemPrompt = PromptManager.RenderPromptTemplate(PromptConstants.CodeAssistantTemplate, null);

        var systemCodeAssistPrompt = PromptManager.RenderPromptTemplate(
            PromptConstants.CodeAssistantTemplate,
            new { codeContext }
        );

        var systemMessagesTokenCount = TokenizerHelper.TokenCount(systemPrompt);
        var userMessagesTokenCount = TokenizerHelper.TokenCount(systemCodeAssistPrompt);
        var codeContextTokenCount = TokenizerHelper.TokenCount(codeContext);
        var historyMessagesTokenCount = 1;
        logger.LogInformation("system messages token count is {Count}k", systemMessagesTokenCount / 1024);
        logger.LogInformation("user messages token count is {Count}k", userMessagesTokenCount / 1024);
        logger.LogInformation("code context token count is {Count}k", codeContextTokenCount / 1024);
        logger.LogInformation("history messages token count is {Count}k", historyMessagesTokenCount / 1024);

        // https://platform.openai.com/docs/guides/text-generation/building-prompts
        var requestBody = new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = RoleType.System.Humanize(LetterCasing.LowerCase), content = systemCodeAssistPrompt },
                // Histories: TODO
                new { role = RoleType.User.Humanize(LetterCasing.LowerCase), content = userQuery },
            },
            temperature = 0.2,
            max_tokens = _options.MaxTokenSize,
        };

        logger.LogInformation("Sending completion request to LLaMA");

        // https://ollama.com/blog/openai-compatibility
        // https://www.youtube.com/watch?v=38jlvmBdBrU
        // https://platform.openai.com/docs/api-reference/chat/create
        // https://github.com/ollama/ollama/blob/main/docs/api.md#generate-a-chat-completion
        var response = await client.PostAsJsonAsync("v1/chat/completions", requestBody);

        // response should add to history with role `assistant`
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation(
                "Received successful response from LLaMA. ResponseContent: {ResponseContent}",
                responseContent
            );
        }
        else
        {
            logger.LogError("Error in LLaMA completion response: {ResponseContent}", responseContent);
        }

        var completionResponse = JsonSerializer.Deserialize<LlamaCompletionResponse>(responseContent);

        return completionResponse?.Choices.FirstOrDefault()?.Text.Trim() ?? string.Empty;
    }

    public async Task<IList<double>> GetEmbeddingAsync(string input)
    {
        var requestBody = new
        {
            input = new[] { input },
            model = _options.EmbeddingsModel,
            encoding_format = "float",
        };

        var count = TokenizerHelper.TokenCount(input);

        logger.LogInformation("Token is {Count}k", count / 1024);

        logger.LogInformation("Sending embedding request to LLaMA");

        // https://github.com/ollama/ollama/blob/main/docs/api.md#generate-embeddings
        // https://platform.openai.com/docs/api-reference/embeddings
        // https://ollama.com/blog/embedding-models
        var response = await client.PostAsJsonAsync("v1/embeddings", requestBody);

        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation(
                "Received successful embedding response from LLaMA. ResponseContent: {ResponseContent}",
                responseContent
            );
        }
        else
        {
            logger.LogError("Error in LLaMA embedding response: {ResponseContent}", responseContent);
        }

        var embeddingResponse = JsonSerializer.Deserialize<LlamaEmbeddingResponse>(responseContent);

        return embeddingResponse?.Data.FirstOrDefault()?.Embedding ?? new List<double>();
    }
}
