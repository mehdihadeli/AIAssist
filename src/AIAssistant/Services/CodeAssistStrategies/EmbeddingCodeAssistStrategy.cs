using AIAssistant.Contracts;
using Clients.Chat.Models;
using Clients.Models;
using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssistant.Services.CodeAssistStrategies;

public class EmbeddingCodeAssistStrategy(
    CodeLoaderService codeLoaderService,
    EmbeddingService embeddingService,
    CodeFileMapService codeFileMapService,
    ILLMClientManager llmClientManager
) : ICodeStrategy
{
    private ChatSession _chatSession = default!;

    public async Task LoadCodeFiles(
        ChatSession chatSession,
        string? contextWorkingDirectory,
        IEnumerable<string>? extraCodeFiles = null
    )
    {
        _chatSession = chatSession;

        var treeSitterCodeCaptures = codeLoaderService.LoadTreeSitterCodeCaptures(
            contextWorkingDirectory,
            extraCodeFiles
        );

        if (!treeSitterCodeCaptures.Any())
            throw new Exception("Not found any files to load.");

        var codeFilesMap = codeFileMapService.GenerateCodeFileMaps(treeSitterCodeCaptures);

        // generate embeddings data with using llms embeddings apis
        // https://ollama.com/blog/embedding-models
        // https://github.com/microsoft/semantic-kernel/blob/main/dotnet/notebooks/06-memory-and-embeddings.ipynb
        // https://github.com/chroma-core/chroma
        await embeddingService.AddEmbeddingsForFiles(codeFilesMap, chatSession.SessionId);
    }

    public async IAsyncEnumerable<string?> QueryAsync(string userQuery)
    {
        var relatedEmbeddings = await embeddingService.GetRelatedEmbeddings(userQuery, _chatSession.SessionId);

        // Prepare context from relevant code snippets
        var codeContext = embeddingService.CreateLLMContext(relatedEmbeddings);

        // Generate a response from the language model (e.g., OpenAI or Llama)
        var completionStreams = llmClientManager.GetCompletionStreamAsync(_chatSession, userQuery, codeContext);

        await foreach (var streamItem in completionStreams)
        {
            yield return streamItem;
        }
    }
}
