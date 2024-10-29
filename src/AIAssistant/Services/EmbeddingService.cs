using AIAssistant.Contracts;
using AIAssistant.Data;
using AIAssistant.Models;
using AIAssistant.Prompts;
using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssistant.Services;

public class EmbeddingService(ILLMClientManager llmClientManager, EmbeddingsStore embeddingsStore)
{
    public async Task AddEmbeddingsForFiles(IEnumerable<CodeFileMap> codeFilesMap, Guid sessionId)
    {
        IList<CodeEmbedding> codeEmbeddings = new List<CodeEmbedding>();
        foreach (var codeFileMap in codeFilesMap)
        {
            var input = GenerateEmbeddingInputString(codeFileMap.TreeSitterFullCode);
            var embeddingData = await llmClientManager.GetEmbeddingAsync(input);

            codeEmbeddings.Add(
                new CodeEmbedding
                {
                    RelativeFilePath = codeFileMap.RelativePath,
                    TreeSitterFullCode = codeFileMap.TreeSitterFullCode,
                    TreeOriginalCode = codeFileMap.TreeOriginalCode,
                    Code = codeFileMap.OriginalCode,
                    SessionId = sessionId,
                    Embeddings = embeddingData,
                }
            );
        }

        // we can replace it with an embedded database like `chromadb`, it can give us n of most similarity items
        await embeddingsStore.AddCodeEmbeddings(codeEmbeddings);
    }

    public async Task<IEnumerable<CodeEmbedding>> GetRelatedEmbeddings(string userQuery, Guid sessionId)
    {
        // Generate embedding for user input based on LLM apis
        var userEmbedding = await GenerateEmbeddingForUserInput(userQuery);

        // Find relevant code based on the user query
        var relevantCodes = embeddingsStore.Query(userEmbedding, sessionId, llmClientManager.EmbeddingThreshold);

        return relevantCodes;
    }

    public async Task<IList<double>> GenerateEmbeddingForUserInput(string userInput)
    {
        return await llmClientManager.GetEmbeddingAsync(userInput);
    }

    public string CreateLLMContext(IEnumerable<CodeEmbedding> relevantCode)
    {
        return string.Join(
            Environment.NewLine + Environment.NewLine,
            relevantCode.Select(rc =>
                PromptManager.RenderPromptTemplate(
                    AIAssistantConstants.Prompts.CodeBlockTemplate,
                    new { treeSitterCode = rc.TreeOriginalCode, relativeFilePath = rc.RelativeFilePath }
                )
            )
        );
    }

    private static string GenerateEmbeddingInputString(string treeSitterCode)
    {
        return PromptManager.RenderPromptTemplate(
            AIAssistantConstants.Prompts.CodeEmbeddingTemplate,
            new { treeSitterCode = treeSitterCode }
        );
    }
}
