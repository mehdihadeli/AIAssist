using AIRefactorAssistant.Models;
using Clients;
using Microsoft.Extensions.Logging;

namespace AIRefactorAssistant.Services;

public class EmbeddingService(ILanguageModelService languageModelService, ILogger<EmbeddingService> logger)
{
    public async Task<IList<CodeEmbedding>> GenerateEmbeddingsForCode(IList<CodeEmbedding> codeChunks)
    {
        logger.LogInformation("Started generating embeddings for code");

        var embeddings = new List<CodeEmbedding>();

        foreach (var codeChunk in codeChunks)
        {
            var embedding = await languageModelService.GetEmbeddingAsync(codeChunk.Chunk);
            embeddings.Add(
                new CodeEmbedding
                {
                    Embedding = embedding,
                    Chunk = codeChunk.Chunk,
                    RelativeFilePath = codeChunk.RelativeFilePath,
                }
            );
        }

        logger.LogInformation("Embeddings generated for all code chunks");
        return embeddings;
    }

    public async Task<string> GenerateEmbeddingForUserInput(string userInput)
    {
        return await languageModelService.GetEmbeddingAsync(userInput);
    }
}
