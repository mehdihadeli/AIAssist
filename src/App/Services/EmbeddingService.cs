using AIRefactorAssistant.Data;
using AIRefactorAssistant.Models;
using BuildingBlocks.Utils;
using Clients;
using Clients.Prompts;
using Microsoft.Extensions.Logging;

namespace AIRefactorAssistant.Services;

public class EmbeddingService(
    ILanguageModelService languageModelService,
    EmbeddingsStore embeddingsStore,
    ILogger<EmbeddingService> logger
)
{
    public async Task<IReadOnlyList<CodeEmbedding>> GenerateEmbeddingsForCodeFiles(
        IEnumerable<ApplicationCode> applicationCodes,
        Guid sessionId
    )
    {
        logger.LogInformation("Started generating embeddings for code");

        var codeEmbeddings = new List<CodeEmbedding>();
        foreach (var applicationCode in applicationCodes)
        {
            var input = GenerateInputString(applicationCode);
            var embeddingData = await languageModelService.GetEmbeddingAsync(input);
            codeEmbeddings.Add(
                new CodeEmbedding
                {
                    RelativeFilePath = applicationCode.RelativePath,
                    Code = applicationCode.Code,
                    EmbeddingData = embeddingData,
                    MethodsName = applicationCode.MethodsName,
                    ClassName = applicationCode.ClassesName,
                    SessionId = sessionId,
                }
            );
        }

        logger.LogInformation("Code Embeddings generated from all files via llm.");

        return codeEmbeddings.AsReadOnly();
    }

    public async Task<string> GenerateEmbeddingForUserInput(string userInput)
    {
        return await languageModelService.GetEmbeddingAsync(userInput);
    }

    public IEnumerable<CodeEmbedding> FindMostRelevantCode(string userEmbedding, Guid sessionId)
    {
        return embeddingsStore.FindMostRelevantCode(userEmbedding, sessionId).Take(3);
    }

    public string PrepareLLmContextCodeEmbeddings(IEnumerable<CodeEmbedding> relevantCode)
    {
        return string.Join(
            Environment.NewLine + Environment.NewLine,
            relevantCode.Select(rc =>
            {
                var mdLanguage = MdCodeBlockHelper.GetMdLanguage(rc.RelativeFilePath);
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

    private static string GenerateInputString(ApplicationCode applicationCode)
    {
        return PromptManager.RenderPromptTemplate(
            PromptConstants.CodeEmbeddingTemplate,
            new
            {
                relativePath = applicationCode.RelativePath,
                code = applicationCode.Code,
                classesName = applicationCode.ClassesName,
                methodsName = applicationCode.MethodsName,
            }
        );

        // return JsonSerializer.Serialize(
        //     new
        //     {
        //         ClassName = applicationCode.ClassName,
        //         MethodNames = applicationCode.MethodsName,
        //         RelativeFilePath = applicationCode.RelativeFilePath,
        //         Code = applicationCode.Code,
        //     },
        //     new JsonSerializerOptions { WriteIndented = true }
        // );
    }
}
