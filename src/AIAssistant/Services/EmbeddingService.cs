using AIAssistant.Chat.Models;
using AIAssistant.Contracts;
using AIAssistant.Data;
using AIAssistant.Dtos;
using AIAssistant.Models;
using BuildingBlocks.LLM;
using BuildingBlocks.Utils;
using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssistant.Services;

public class EmbeddingService(
    ILLMClientManager llmClientManager,
    ICodeEmbeddingsRepository codeEmbeddingsRepository,
    IPromptManager promptManager
) : IEmbeddingService
{
    public async Task<AddEmbeddingsForFilesResult> AddOrUpdateEmbeddingsForFiles(
        IEnumerable<CodeFileMap> codeFilesMap,
        ChatSession chatSession
    )
    {
        int totalTokens = 0;
        decimal totalCost = 0;

        IList<CodeEmbedding> codeEmbeddings = new List<CodeEmbedding>();

        foreach (var codeFileMap in codeFilesMap)
        {
            var input = promptManager.GetEmbeddingInputString(codeFileMap.TreeSitterFullCode);
            var embeddingResult = await llmClientManager.GetEmbeddingAsync(input, codeFileMap.RelativePath);

            codeEmbeddings.Add(
                new CodeEmbedding
                {
                    RelativeFilePath = codeFileMap.RelativePath,
                    TreeSitterFullCode = codeFileMap.TreeSitterFullCode,
                    TreeOriginalCode = codeFileMap.TreeOriginalCode,
                    Code = codeFileMap.OriginalCode,
                    SessionId = chatSession.SessionId,
                    Embeddings = embeddingResult.Embeddings,
                }
            );

            totalTokens += embeddingResult.TotalTokensCount;
            totalCost += embeddingResult.TotalCost;
        }

        // we can replace it with an embedded database like `chromadb`, it can give us n of most similarity items
        await codeEmbeddingsRepository.AddOrUpdateCodeEmbeddings(codeEmbeddings);

        return new AddEmbeddingsForFilesResult(totalTokens, totalCost);
    }

    public async Task<GetRelatedEmbeddingsResult> GetRelatedEmbeddings(string userQuery, ChatSession chatSession)
    {
        // Generate embedding for user input based on LLM apis
        var embeddingsResult = await GenerateEmbeddingForUserInput(userQuery);

        // Find relevant code based on the user query
        var relevantCodes = codeEmbeddingsRepository.Query(
            embeddingsResult.Embeddings,
            chatSession.SessionId,
            llmClientManager.EmbeddingThreshold
        );

        return new GetRelatedEmbeddingsResult(
            relevantCodes,
            embeddingsResult.TotalTokensCount,
            embeddingsResult.TotalCost
        );
    }

    public IEnumerable<CodeEmbedding> QueryByFilter(
        ChatSession chatSession,
        Func<CodeEmbeddingDocument, bool>? documentFilter = null,
        IDictionary<string, string>? metadataFilter = null
    )
    {
        return codeEmbeddingsRepository.QueryByDocumentFilter(chatSession.SessionId, documentFilter, metadataFilter);
    }

    public async Task<GetEmbeddingResult> GenerateEmbeddingForUserInput(string userInput)
    {
        return await llmClientManager.GetEmbeddingAsync(userInput, null);
    }
}
