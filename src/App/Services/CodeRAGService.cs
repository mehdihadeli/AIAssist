using AIRefactorAssistant.Data;
using Clients;
using Microsoft.Extensions.Logging;

namespace AIRefactorAssistant.Services;

public class CodeRAGService(
    CodeLoaderService codeLoaderService,
    EmbeddingService embeddingService,
    ILanguageModelService languageModelService,
    CodeRefactorService codeRefactorService,
    EmbeddingsStore embeddingsStore,
    ILogger<CodeRAGService> logger
)
{
    // private IEnumerable<CodeEmbedding> _codeEmbeddings = new List<CodeEmbedding>();

    private readonly Guid _sessionId = Guid.NewGuid();

    public async Task InitializeNewSession(string codePath)
    {
        logger.LogInformation("load application code...");

        var applicationCodes = codeLoaderService.LoadApplicationCodes(codePath);

        // generate embeddings data with using llms embeddings apis
        var codeEmbeddings = await embeddingService.GenerateEmbeddingsForCodeFiles(applicationCodes, _sessionId);

        await embeddingsStore.AddCodeEmbeddings(codeEmbeddings);
    }

    public async Task<string> ProcessUserRequestAsync(string userQuery)
    {
        // Generate embedding for user input
        var userEmbedding = await embeddingService.GenerateEmbeddingForUserInput(userQuery);

        // Find relevant code based on the user query
        var relevantCodes = embeddingService.FindMostRelevantCode(userEmbedding, _sessionId);

        // Prepare context from relevant code snippets
        var llmContext = embeddingService.PrepareLLmContextCodeEmbeddings(relevantCodes);

        // Generate a response from the language model (e.g., OpenAI or Llama)
        var completion = await languageModelService.GetCompletionAsync(userQuery, llmContext);

        return completion;
    }

    public void ApplyChangesToCodeBase(string completion)
    {
        // Extract file paths and chunks from relevant code embeddings
        var relevantCode = _codeEmbeddings.ToList();

        codeRefactorService.ApplyChangesToCodeBase(relevantCode, completion);
    }
}
