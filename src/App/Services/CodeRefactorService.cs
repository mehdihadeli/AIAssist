using AIRefactorAssistant.Models;
using BuildingBlocks.Utils;
using Clients;
using Clients.Prompts;
using Microsoft.Extensions.Logging;

namespace AIRefactorAssistant.Services;

public class CodeRefactorService(ILanguageModelService languageModelService, ILogger<CodeRefactorService> logger)
{
    public void ApplyChangesToCodeBase(IList<CodeEmbedding> relevantCodes, string completion)
    {
        var rootPath = Directory.GetCurrentDirectory();

        foreach (var relevantCode in relevantCodes)
        {
            var oldChunk = relevantCode.Code;
            var relevantFilePath = relevantCode.RelativeFilePath;

            var filePath = Path.Combine(rootPath, relevantFilePath);
            var fileContent = File.ReadAllText(filePath);

            if (fileContent.Contains(oldChunk, StringComparison.InvariantCulture))
            {
                var updatedContent = fileContent.Replace(oldChunk, completion, StringComparison.InvariantCulture);

                File.WriteAllText(filePath, updatedContent);
                logger.LogInformation("Changes applied to {RelevantFilePath}", relevantFilePath);
            }
            else
            {
                logger.LogError(
                    "Could not find the code chunk in {RelevantFilePath}. Changes not applied.",
                    relevantFilePath
                );
            }
        }
    }
}
