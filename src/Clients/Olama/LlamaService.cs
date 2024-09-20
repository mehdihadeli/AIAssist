using System.Net.Http.Json;
using Clients.Olama.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Clients.Olama;

public class LlamaService(HttpClient client, ILogger<LlamaService> logger) : ILanguageModelService
{
    public async Task<string> GetCompletionAsync(string prompt, string context)
    {
        var requestBody = new
        {
            model = "llama3.1",
            prompt = $"The following is code from the application: {
                                  context
                              }. Based on the user's input: {
                                  prompt
                              }, suggest changes or enhancements.",
            max_tokens = 150,
            temperature = 0.5,
        };

        logger.LogInformation("Sending completion request to LLaMA");

        // https://platform.openai.com/docs/api-reference
        // https://ollama.com/blog/openai-compatibility
        // https://www.youtube.com/watch?v=38jlvmBdBrU

        var response = await client.PostAsJsonAsync("v1/chat/completions", requestBody);

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

        var completionResponse = JsonConvert.DeserializeObject<LlamaCompletionResponse>(responseContent);

        return completionResponse?.Choices.FirstOrDefault()?.Text.Trim() ?? string.Empty;
    }

    public async Task<string> GetEmbeddingAsync(string text)
    {
        var requestBody = new { input = text, model = "llama3.1" };

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

        var embeddingResponse = JsonConvert.DeserializeObject<LlamaEmbeddingResponse>(responseContent);

        return string.Join(",", embeddingResponse?.Data.FirstOrDefault()?.Embedding ?? new List<double>());
    }
}
