using System.Net.Http.Json;
using BuildingBlocks.LLM;
using Clients.Ollama.Models;
using Clients.Prompts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Clients.Anthropic;

public class AnthropicService(HttpClient client, ILogger<AnthropicService> logger, IOptions<AnthropicOptions> options)
    : ILanguageModelService
{
    private readonly AnthropicOptions _options = options.Value;

    public async Task<string> GetCompletionAsync(string userQuery, string codeContext)
    {
        var systemCodeAssistPrompt = PromptManager.RenderPromptTemplate(
            PromptConstants.CodeAssistantTemplate,
            new { codeContext, userQuery }
        );

        // https://platform.openai.com/docs/api-reference/chat/create
        var requestBody = new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = "system", content = systemCodeAssistPrompt },
                new { role = "user", content = userQuery },
            },
            temperature = 0.2,
            max_tokens_to_sample = _options.MaxTokenSize,
        };

        var count = TokenizerHelper.TokenCount(string.Join(',', requestBody.messages.Select(x => x.content)));
        logger.LogInformation("Token is {Count}k", count / 1024);

        logger.LogInformation("Sending completion request to Anthropic");

        // https://platform.openai.com/docs/api-reference/chat/create
        var response = await client.PostAsJsonAsync("v1/complete", requestBody);

        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation(
                "Received successful response from Anthropic. ResponseContent: {ResponseContent}",
                responseContent
            );
        }
        else
        {
            logger.LogError("Error in Anthropic completion response: {ResponseContent}", responseContent);
        }

        var completionResponse = JsonSerializer.Deserialize<LlamaCompletionResponse>(responseContent);

        return completionResponse?.Choices.FirstOrDefault()?.Text.Trim() ?? string.Empty;
    }

    public async Task<IList<double>> GetEmbeddingAsync(string input)
    {
        // https://docs.anthropic.com/en/docs/build-with-claude/embeddings#getting-started-with-voyage-ai
        var requestBody = new { input = new[] { input }, model = _options.EmbeddingsModel };

        var count = TokenizerHelper.TokenCount(input);
        logger.LogInformation("Token is {Count}k", count / 1024);

        logger.LogInformation("Sending embedding request to LLaMA");

        var response = await client.PostAsJsonAsync("v1/embeddings", requestBody); // Adjust endpoint as needed

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
