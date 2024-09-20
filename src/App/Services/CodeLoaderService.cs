using AIRefactorAssistant.Models;
using Microsoft.Extensions.Logging;

namespace AIRefactorAssistant.Services;

public class CodeLoaderService(ILogger<CodeLoaderService> logger)
{
    public IList<CodeEmbedding> LoadApplicationCode(string appRelativePath)
    {
        var rootPath = Directory.GetCurrentDirectory();
        var appPath = Path.Combine(rootPath, appRelativePath);

        logger.LogInformation("AppPath is: {AppPath}", appPath);

        var csFiles = Directory.GetFiles(appPath, "*.cs", SearchOption.AllDirectories);
        var embeddings = new List<CodeEmbedding>();

        foreach (var file in csFiles)
        {
            var relativePath = Path.GetRelativePath(appPath, file);
            logger.LogInformation("App relative path is: {RelativePath}", relativePath);

            var fileContent = File.ReadAllText(file);
            var chunks = ChunkCode(fileContent);

            foreach (var chunk in chunks)
            {
                // Add chunk without embeddings initially
                embeddings.Add(new CodeEmbedding { Chunk = chunk, RelativeFilePath = relativePath });
            }
        }

        return embeddings;
    }

    private static List<string> ChunkCode(string code, int maxChunkSize = 1000)
    {
        var chunks = new List<string>();
        for (int i = 0; i < code.Length; i += maxChunkSize)
        {
            chunks.Add(code.Substring(i, Math.Min(maxChunkSize, code.Length - i)));
        }
        return chunks;
    }
}
