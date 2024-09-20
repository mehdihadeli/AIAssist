using AIRefactorAssistant.Models;
using Microsoft.Extensions.Logging;

namespace AIRefactorAssistant.Services;

public class LLMManager(
    CodeLoaderService codeLoaderService,
    EmbeddingService embeddingService,
    CodeRefactorService codeRefactorService,
    ILogger<LLMManager> logger
)
{
    private IList<CodeEmbedding> _codeEmbeddings = new List<CodeEmbedding>();

    public async Task InitializeEmbeddingsAsync(string codePath)
    {
        logger.LogInformation("Initializing code embeddings...");

        // Load code chunks
        var codeChunks = codeLoaderService.LoadApplicationCode(codePath);

        // Generate embeddings for code chunks
        _codeEmbeddings = await embeddingService.GenerateEmbeddingsForCode(codeChunks);

        logger.LogInformation("Embeddings initialized successfully.");
    }

    public async Task<string> ProcessUserRequestAsync(string userInput)
    {
        // Generate embedding for user input
        var userEmbedding = await embeddingService.GenerateEmbeddingForUserInput(userInput);

        // Find relevant code based on the user query
        var relevantCodes = codeRefactorService.FindMostRelevantCode(userEmbedding, _codeEmbeddings);

        // Prepare context from relevant code snippets
        var context = codeRefactorService.PrepareContextFromCode(relevantCodes);

        // Generate a response from the language model (e.g., OpenAI or Llama)
        var completion = await codeRefactorService.GenerateRefactoringSuggestions(userInput, context);

        return completion;
    }

    public void ApplyChangesToCodeBase(string completion)
    {
        // Extract file paths and chunks from relevant code embeddings
        var relevantCode = _codeEmbeddings.ToList();

        codeRefactorService.ApplyChangesToCodeBase(relevantCode, completion);
    }
}
