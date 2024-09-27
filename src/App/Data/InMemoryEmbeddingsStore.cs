using AIRefactorAssistant.Models;

namespace AIRefactorAssistant.Data;

public class EmbeddingsStore
{
    private readonly List<CodeEmbedding> _codeEmbeddings = new();

    public Task AddCodeEmbeddings(IReadOnlyList<CodeEmbedding> codeEmbeddings)
    {
        if (codeEmbeddings.Any())
            _codeEmbeddings.AddRange(codeEmbeddings);

        return Task.CompletedTask;
    }

    public IEnumerable<CodeEmbedding> FindMostRelevantCode(string userEmbeddingQuery, Guid sessionId)
    {
        var userEmbeddingVector = userEmbeddingQuery.Split(',').Select(double.Parse).ToArray();

        var relevantCodes = _codeEmbeddings
            .Where(x => x.SessionId == sessionId)
            .Select(ce => (CodeEmbedding: ce, score: CosineSimilarity(ce.EmbeddingData, userEmbeddingVector)))
            .OrderByDescending(ce => ce.score)
            .Select(x => x.CodeEmbedding);

        return relevantCodes.ToList().AsReadOnly();
    }

    public Task ClearEmbeddingsBySession(Guid sessionId)
    {
        _codeEmbeddings.RemoveAll(x => x.SessionId == sessionId);

        return Task.CompletedTask;
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
