using AIRefactorAssistant.Data;
using Clients;
using Clients.Models;
using Microsoft.Extensions.Logging;

namespace AIRefactorAssistant.Services;

public class CodeRAGService(
    CodeLoaderService codeLoaderService,
    EmbeddingService embeddingService,
    ILanguageModelService languageModelService,
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
        // https://ollama.com/blog/embedding-models
        // https://github.com/microsoft/semantic-kernel/blob/main/dotnet/notebooks/06-memory-and-embeddings.ipynb
        // https://github.com/chroma-core/chroma
        var codeEmbeddings = await embeddingService.GenerateEmbeddingsForCodeFiles(applicationCodes, _sessionId);

        // we can replace it with an embedded database like `chromadb`, it can give us n of most similarity items
        await embeddingsStore.AddCodeEmbeddings(codeEmbeddings);
    }

    public async Task<IList<CodeChange>> ProcessUserRequestAsync(string userQuery)
    {
        // Generate embedding for user input based on LLM apis
        var userEmbedding = await embeddingService.GenerateEmbeddingForUserInput(userQuery);

        // Find relevant code based on the user query
        var relevantCodes = embeddingsStore.Query(userEmbedding, _sessionId);

        // Prepare context from relevant code snippets
        var llmContext = embeddingService.PrepareLLmContextCodeEmbeddings(relevantCodes);

        // Generate a response from the language model (e.g., OpenAI or Llama)
        var completion = await languageModelService.GetCompletionAsync(userQuery, llmContext);

        // parse completion to create codeChanges
        var codeChanges = new List<CodeChange>();

        return codeChanges;
    }

    public void ApplyChangesToCodeBase(IList<CodeChange> codeChanges)
    {
        // var rootPath = Directory.GetCurrentDirectory();
        //
        // foreach (var relevantCode in codeChanges)
        // {
        //     var oldChunk = relevantCode.Code;
        //     var relevantFilePath = relevantCode.RelativeFilePath;
        //
        //     var filePath = Path.Combine(rootPath, relevantFilePath);
        //     var fileContent = File.ReadAllText(filePath);
        //
        //     if (fileContent.Contains(oldChunk, StringComparison.InvariantCulture))
        //     {
        //         var updatedContent = fileContent.Replace(oldChunk, completion, StringComparison.InvariantCulture);
        //
        //         File.WriteAllText(filePath, updatedContent);
        //         logger.LogInformation("Changes applied to {RelevantFilePath}", relevantFilePath);
        //     }
        //     else
        //     {
        //         logger.LogError(
        //             "Could not find the code chunk in {RelevantFilePath}. Changes not applied.",
        //             relevantFilePath
        //         );
        //     }
        // }
    }
}
