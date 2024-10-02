using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.LLM;
using Clients.OpenAI.Models;
using Clients.Prompts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Clients.OpenAI;

public class OpenAiService(HttpClient client, ILogger<OpenAiService> logger, IOptions<OpenAIOptions> options)
    : ILanguageModelService
{
    private readonly OpenAIOptions _options = options.Value;

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
            max_tokens = _options.MaxTokenSize,
        };

        var count = TokenizerHelper.TokenCount(string.Join(',', requestBody.messages.Select(x => x.content)));
        logger.LogInformation("Token is {Count}k", count / 1024);

        logger.LogInformation("Sending completion request to OpenAI");

        var response = await client.PostAsJsonAsync("v1/chat/completions", requestBody);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation(
                "Received successful response from OpenAI. ResponseContent: {ResponseContent}",
                responseContent
            );
        }
        else
        {
            logger.LogError("Error in OpenAI completion response: {ResponseContent}", responseContent);
        }

        var completionResponse = JsonSerializer.Deserialize<OpenAiCompletionResponse>(responseContent);

        return completionResponse?.Choices.FirstOrDefault()?.Text.Trim() ?? string.Empty;
    }

    public async Task<IList<double>> GetEmbeddingAsync(string input)
    {
        var requestBody = new { input = new[] { input }, model = _options.EmbeddingsModel };

        var count = TokenizerHelper.TokenCount(input);
        logger.LogInformation("Token is {Count}k", count / 1024);

        logger.LogInformation("Sending embedding request to OpenAI");

        // https://platform.openai.com/docs/api-reference/embeddings
        var response = await client.PostAsJsonAsync("v1/embeddings", requestBody);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation(
                "Received successful embedding response from OpenAI. ResponseContent: {ResponseContent}",
                responseContent
            );
        }
        else
        {
            logger.LogError("Error in OpenAI embedding response: {ResponseContent}", responseContent);
        }

        var embeddingResponse = JsonSerializer.Deserialize<OpenAiEmbeddingResponse>(responseContent);

        return embeddingResponse?.Data.FirstOrDefault()?.Embedding ?? new List<double>();
    }
}
