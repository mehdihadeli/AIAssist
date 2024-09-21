using System.Text.Json;
using AIRefactorAssistant.Models;
using Clients;
using Microsoft.Extensions.Logging;

namespace AIRefactorAssistant.Services;

public class EmbeddingService(ILanguageModelService languageModelService, ILogger<EmbeddingService> logger)
{
    public async Task<IList<CodeEmbedding>> GenerateEmbeddingsForCode(IList<CodeEmbedding> codeEmbeddings)
    {
        logger.LogInformation("Started generating embeddings for code");

        foreach (var codeEmbedding in codeEmbeddings)
        {
            var input = GenerateInputString(codeEmbedding);
            var embeddingData = await languageModelService.GetEmbeddingAsync(input);

            codeEmbedding.EmbeddingData = embeddingData;
        }

        logger.LogInformation("Embeddings data generated for all code chunks");

        return codeEmbeddings;
    }

    public async Task<string> GenerateEmbeddingForUserInput(string userInput)
    {
        return await languageModelService.GetEmbeddingAsync(userInput);
    }

    public static string GenerateInputString(CodeEmbedding codeEmbedding)
    {
        return $@"
        Class: {
            codeEmbedding.ClassName
        }
        Methods: {
            codeEmbedding.MethodsName
        }
        File Path: {
            codeEmbedding.RelativeFilePath
        }

        Code:
        {
            codeEmbedding.Code
        }";

        // return JsonSerializer.Serialize(
        //     new
        //     {
        //         ClassName = codeEmbedding.ClassName,
        //         MethodNames = codeEmbedding.MethodsName,
        //         RelativeFilePath = codeEmbedding.RelativeFilePath,
        //         Code = codeEmbedding.Code,
        //     },
        //     new JsonSerializerOptions { WriteIndented = true }
        // );
    }
}
