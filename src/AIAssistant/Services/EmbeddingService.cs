using AIAssistant.Contracts;
using AIAssistant.Data;
using AIAssistant.Models;
using AIAssistant.Prompts;
using BuildingBlocks.Extensions;
using TreeSitter.Bindings.CustomTypes.TreeParser;
using TreeSitter.Bindings.Utilities;

namespace AIAssistant.Services;

public class EmbeddingService(ILLMServiceManager llmServiceManager, EmbeddingsStore embeddingsStore)
{
    public async Task AddEmbeddingsForFiles(IEnumerable<CodeFile> applicationCodes, Guid sessionId)
    {
        var repositoryMap = new RepositoryMap();
        IList<CodeEmbedding> codeEmbeddings = new List<CodeEmbedding>();
        foreach (var applicationCode in applicationCodes)
        {
            var codeEmbedding = new CodeEmbedding
            {
                RelativeFilePath = applicationCode.RelativePath,
                // TreeSitterCode = TreeSitterRepositoryMapGenerator.GenerateTreeSitterRepositoryMap(
                //     applicationCode.Code,
                //     applicationCode.RelativePath,
                //     repositoryMap,
                //     true
                // ),
                Code = applicationCode.Code,
                SessionId = sessionId,
            };

            var input = GenerateEmbeddingInputString(codeEmbedding);

            var embeddingData = await llmServiceManager.GetEmbeddingAsync(input);
            codeEmbedding.Embeddings = embeddingData;

            codeEmbeddings.Add(codeEmbedding);
        }

        // we can replace it with an embedded database like `chromadb`, it can give us n of most similarity items
        await embeddingsStore.AddCodeEmbeddings(codeEmbeddings);
    }

    public async Task<IEnumerable<CodeEmbedding>> GetRelatedEmbeddings(string userQuery, Guid sessionId)
    {
        // Generate embedding for user input based on LLM apis
        var userEmbedding = await GenerateEmbeddingForUserInput(userQuery);

        // Find relevant code based on the user query
        var relevantCodes = embeddingsStore.Query(userEmbedding, sessionId);

        return relevantCodes;
    }

    public async Task<IList<double>> GenerateEmbeddingForUserInput(string userInput)
    {
        return await llmServiceManager.GetEmbeddingAsync(userInput);
    }

    public string CreateLLMContext(IEnumerable<CodeEmbedding> relevantCode)
    {
        return string.Join(
            Environment.NewLine + Environment.NewLine,
            relevantCode.Select(rc =>
            {
                var mdLanguage = rc.RelativeFilePath.GetMdLanguageFromFilePath();
                string fileName = Path.GetFileName(rc.RelativeFilePath);

                return PromptManager.RenderPromptTemplate(
                    PromptConstants.CodeBlockTemplate,
                    new
                    {
                        relativeFilePath = rc.RelativeFilePath,
                        code = rc.Code,
                        fileName,
                        mdLanguage,
                    }
                );
            })
        );
    }

    private static string GenerateEmbeddingInputString(CodeEmbedding codeEmbedding)
    {
        return PromptManager.RenderPromptTemplate(
            PromptConstants.CodeEmbeddingTemplate,
            new { relativePath = codeEmbedding.RelativeFilePath, treeSitterCode = codeEmbedding.TreeSitterCode }
        );
    }
}
