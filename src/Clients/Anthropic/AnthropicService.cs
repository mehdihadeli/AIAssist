using System.Net.Http.Json;
using Clients.Olama.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Clients.Anthropic;

public class AnthropicService(HttpClient client, ILogger<AnthropicService> logger) : ILanguageModelService
{
    public async Task<string> GetCompletionAsync(string userQuery, string codeContext)
    {
        var userContent = PromptHelper.GenerateSuggestedCodePrompt(codeContext, userQuery);

        var requestBody = new
        {
            model = "claude-2.1",
            prompt = userContent,
            max_tokens_to_sample = 2024,
            temperature = 0.5,
        };

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

        var completionResponse = JsonConvert.DeserializeObject<LlamaCompletionResponse>(responseContent);

        return completionResponse?.Choices.FirstOrDefault()?.Text.Trim() ?? string.Empty;
    }

    public async Task<string> GetEmbeddingAsync(string input)
    {
        var requestBody = new { input = new[] { input }, model = "voyage-2" };

        logger.LogInformation("Sending embedding request to LLaMA");

        // https://docs.anthropic.com/en/docs/build-with-claude/embeddings
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

        var embeddingResponse = JsonConvert.DeserializeObject<LlamaEmbeddingResponse>(responseContent);

        return string.Join(",", embeddingResponse?.Data.FirstOrDefault()?.Embedding ?? new List<double>());
    }
}
