using AIAssistant.Contracts;
using AIAssistant.Models;
using AIAssistant.Models.Options;
using Clients.Chat.Models;
using Microsoft.Extensions.Options;

namespace AIAssistant.Services.CodeAssistStrategies;

public class EmbeddingCodeAssistStrategy(
    CodeLoaderService codeLoaderService,
    EmbeddingService embeddingService,
    CodeFileMapService codeFileMapService,
    ILLMClientManager llmClientManager,
    IPromptStorage promptStorage,
    IOptions<CodeAssistOptions> options
) : ICodeStrategy
{
    private ChatSession _chatSession = default!;

    public async Task LoadCodeFiles(
        ChatSession chatSession,
        string? contextWorkingDirectory,
        IEnumerable<string>? codeFiles
    )
    {
        _chatSession = chatSession;

        var treeSitterCodeCaptures = codeLoaderService.LoadTreeSitterCodeCaptures(contextWorkingDirectory, codeFiles);

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

        var systemCodeAssistPrompt = promptStorage.GetPrompt(
            CommandType.Code,
            options.Value.DiffType,
            new { codeContext = codeContext }
        );

        // Generate a response from the language model (e.g., OpenAI or Llama)
        var completionStreams = llmClientManager.GetCompletionStreamAsync(
            _chatSession,
            userQuery: userQuery,
            systemContext: systemCodeAssistPrompt
        );

        await foreach (var streamItem in completionStreams)
        {
            yield return streamItem;
        }
    }
}
