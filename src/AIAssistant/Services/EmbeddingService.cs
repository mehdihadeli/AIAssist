using AIAssistant.Contracts;
using AIAssistant.Data;
using AIAssistant.Dtos;
using AIAssistant.Models;
using AIAssistant.Prompts;
using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssistant.Services;

public class EmbeddingService(
    ILLMClientManager llmClientManager,
    EmbeddingsStore embeddingsStore,
    IChatSessionManager chatSessionManager
) : IEmbeddingService
{
    public async Task<AddEmbeddingsForFilesResult> AddEmbeddingsForFiles(IEnumerable<CodeFileMap> codeFilesMap)
    {
        int totalTokens = 0;
        decimal totalCost = 0;
        var session = chatSessionManager.GetCurrentActiveSession();

        IList<CodeEmbedding> codeEmbeddings = new List<CodeEmbedding>();
        foreach (var codeFileMap in codeFilesMap)
        {
            var input = GenerateEmbeddingInputString(codeFileMap.TreeSitterFullCode);
            var embeddingResult = await llmClientManager.GetEmbeddingAsync(input);

            codeEmbeddings.Add(
                new CodeEmbedding
                {
                    RelativeFilePath = codeFileMap.RelativePath,
                    TreeSitterFullCode = codeFileMap.TreeSitterFullCode,
                    TreeOriginalCode = codeFileMap.TreeOriginalCode,
                    Code = codeFileMap.OriginalCode,
                    SessionId = session.SessionId,
                    Embeddings = embeddingResult.Embeddings,
                }
            );

            totalTokens += embeddingResult.TotalTokensCount;
            totalCost += embeddingResult.TotalCost;
        }

        // we can replace it with an embedded database like `chromadb`, it can give us n of most similarity items
        await embeddingsStore.AddCodeEmbeddings(codeEmbeddings);

        return new AddEmbeddingsForFilesResult(totalTokens, totalCost);
    }

    public async Task<GetRelatedEmbeddingsResult> GetRelatedEmbeddings(string userQuery)
    {
        // Generate embedding for user input based on LLM apis
        var embeddingsResult = await GenerateEmbeddingForUserInput(userQuery);

        // Find relevant code based on the user query
        var relevantCodes = embeddingsStore.Query(
            embeddingsResult.Embeddings,
            chatSessionManager.GetCurrentActiveSession().SessionId,
            llmClientManager.EmbeddingThreshold
        );

        return new GetRelatedEmbeddingsResult(
            relevantCodes,
            embeddingsResult.TotalTokensCount,
            embeddingsResult.TotalCost
        );
    }

    public async Task<GetEmbeddingResult> GenerateEmbeddingForUserInput(string userInput)
    {
        return await llmClientManager.GetEmbeddingAsync(userInput);
    }

    private static string GenerateEmbeddingInputString(string treeSitterCode)
    {
        return PromptManager.RenderPromptTemplate(
            AIAssistantConstants.Prompts.CodeEmbeddingTemplate,
            new { treeSitterCode = treeSitterCode }
        );
    }
}
