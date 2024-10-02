using AIRefactorAssistant.Models;
using BuildingBlocks.Utils;
using Clients;
using Clients.Prompts;
using Microsoft.Extensions.Logging;

namespace AIRefactorAssistant.Services;

public class EmbeddingService(ILanguageModelService languageModelService, ILogger<EmbeddingService> logger)
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
            var codeEmbedding = new CodeEmbedding
            {
                RelativeFilePath = applicationCode.RelativePath,
                Code = applicationCode.Code,
                MethodsName = applicationCode.MethodsName,
                ClassesName = applicationCode.ClassesName,
                SessionId = sessionId,
            };

            var input = GenerateEmbeddingInputString(codeEmbedding);
            var embeddingData = await languageModelService.GetEmbeddingAsync(input);

            ArgumentNullException.ThrowIfNull(embeddingData);

            codeEmbedding.Embeddings = embeddingData;

            codeEmbeddings.Add(codeEmbedding);
        }

        logger.LogInformation("Code Embeddings generated from all files via llm.");

        return codeEmbeddings.AsReadOnly();
    }

    public async Task<IList<double>> GenerateEmbeddingForUserInput(string userInput)
    {
        return await languageModelService.GetEmbeddingAsync(userInput);
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

    private static string GenerateEmbeddingInputString(CodeEmbedding codeEmbedding)
    {
        return PromptManager.RenderPromptTemplate(
            PromptConstants.CodeEmbeddingTemplate,
            new
            {
                id = codeEmbedding.Id,
                relativePath = codeEmbedding.RelativeFilePath,
                code = codeEmbedding.Code,
                methodsName = codeEmbedding.MethodsName,
                classesName = codeEmbedding.ClassesName,
            }
        );
    }
}
