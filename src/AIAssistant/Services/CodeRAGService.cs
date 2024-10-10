using AIAssistant.Options;
using Clients.Models;
using Microsoft.Extensions.Options;

namespace AIAssistant.Services;

public class CodeRAGService(
    CodeLoaderService codeLoaderService,
    EmbeddingService embeddingService,
    IOptions<CodeAssistOptions> codeAssistOptions
)
{
    private readonly CodeAssistOptions _codeAssistOptions = codeAssistOptions.Value;

    public async Task Initialize(ChatSession chatSession)
    {
        var applicationCodes = codeLoaderService.LoadApplicationCodes(_codeAssistOptions.ContextWorkingDirectory);

        // generate embeddings data with using llms embeddings apis
        // https://ollama.com/blog/embedding-models
        // https://github.com/microsoft/semantic-kernel/blob/main/dotnet/notebooks/06-memory-and-embeddings.ipynb
        // https://github.com/chroma-core/chroma
        await embeddingService.AddEmbeddingsForFiles(applicationCodes, chatSession.SessionId);
    }

    public async Task<IList<CodeChange>> ModifyOrAddCodeAsync(ChatSession chatSession, string userQuery)
    {
        var relatedEmbeddings = await embeddingService.GetRelatedEmbeddings(userQuery, chatSession.SessionId);

        // Prepare context from relevant code snippets
        var codeContext = embeddingService.CreateLLMContext(relatedEmbeddings);

        // // Generate a response from the language model (e.g., OpenAI or Llama)
        // var completion = await illmServiceManager.GetCompletionAsync(chatSession, userQuery, codeContext);
        //
        // if (string.IsNullOrEmpty(completion))
        //     return new List<CodeChange>();
        //
        // chatSession.CreateAssistantChatItem(completion);
        //
        // var codeChangeResponse = ResponseParser.GetCodeChangesFromResponse(completion);
        //
        // // if (codeChangeResponse is null)
        // // {
        // //     return new List<CodeChange>
        // //            {new CodeChange()
        // //             {
        // //                 Code =
        // //             }};
        // // }
        // //
        // // codeChangeResponse.CodeChanges
        //
        // // parse completion to create codeChanges
        // var codeChanges = new List<CodeChange>();
        //
        // return codeChanges;

        return null;
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
