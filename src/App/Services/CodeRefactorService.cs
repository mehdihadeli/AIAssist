using AIRefactorAssistant.Models;
using Clients;
using Microsoft.Extensions.Logging;

namespace AIRefactorAssistant.Services;

public class CodeRefactorService(ILanguageModelService languageModelService, ILogger<CodeRefactorService> logger)
{
    public IList<CodeEmbedding> FindMostRelevantCode(string userEmbedding, IList<CodeEmbedding> codeEmbeddings)
    {
        var userEmbeddingVector = userEmbedding.Split(',').Select(double.Parse).ToArray();

        var relevantCodes = codeEmbeddings
            .Select(ce => (CodeEmbedding: ce, score: CosineSimilarity(ce.Embedding, userEmbeddingVector)))
            .OrderByDescending(ce => ce.score)
            .Select(x => x.CodeEmbedding)
            .Take(3)
            .ToList();

        return relevantCodes;
    }

    public string PrepareContextFromCode(IList<CodeEmbedding> relevantCode)
    {
        return string.Join(Environment.NewLine + Environment.NewLine, relevantCode.Select(rc => rc.Chunk));
    }

    public async Task<string> GenerateRefactoringSuggestions(string userInput, string context)
    {
        return await languageModelService.GetCompletionAsync(userInput, context);
    }

    public void ApplyChangesToCodeBase(IList<CodeEmbedding> relevantCodes, string completion)
    {
        var rootPath = Directory.GetCurrentDirectory();

        foreach (var relevantCode in relevantCodes)
        {
            var oldChunk = relevantCode.Chunk;
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

    private static double CosineSimilarity(string embedding, double[] userEmbeddingVector)
    {
        var embeddingVector = embedding.Split(',').Select(double.Parse).ToArray();
        double dotProduct = embeddingVector.Zip(userEmbeddingVector, (a, b) => a * b).Sum();
        double magnitudeA = Math.Sqrt(embeddingVector.Select(a => a * a).Sum());
        double magnitudeB = Math.Sqrt(userEmbeddingVector.Select(b => b * b).Sum());

        return dotProduct / (magnitudeA * magnitudeB);
    }
}
