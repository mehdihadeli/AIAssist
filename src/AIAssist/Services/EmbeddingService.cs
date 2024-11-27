using AIAssist.Chat.Models;
using AIAssist.Contracts;
using AIAssist.Data;
using AIAssist.Dtos;
using AIAssist.Models;
using BuildingBlocks.LLM;
using TreeSitter.Bindings.CustomTypes.TreeParser;

namespace AIAssist.Services;

public class EmbeddingService(
    ILLMClientManager llmClientManager,
    ICodeEmbeddingsRepository codeEmbeddingsRepository,
    IPromptManager promptManager,
    ITokenizer tokenizer
) : IEmbeddingService
{
    public async Task<AddEmbeddingsForFilesResult> AddOrUpdateEmbeddingsForFiles(
        IList<CodeFileMap> codeFilesMap,
        ChatSession chatSession
    )
    {
        int totalTokens = 0;
        decimal totalCost = 0;

        var fileEmbeddingsMap = new Dictionary<string, List<IList<double>>>(StringComparer.Ordinal);

        // Group files and manage batching using the updated tokenizer logic
        var fileBatches = await BatchFilesByTokenLimitAsync(codeFilesMap, maxBatchTokens: 8192).ConfigureAwait(false);

        foreach (var batch in fileBatches)
        {
            var batchInputs = batch.GetBatchInputs();
            var embeddingResult = await llmClientManager.GetEmbeddingAsync(batchInputs, null).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

            int resultIndex = 0;
            foreach (var fileChunkGroup in batch.Files)
            {
                // Extract embeddings for the current file's chunks
                var fileEmbeddings = embeddingResult
                    .Embeddings.Skip(resultIndex)
                    .Take(fileChunkGroup.Chunks.Count)
                    .ToList();

                resultIndex += fileChunkGroup.Chunks.Count;

                // Group embeddings by file path
                if (!fileEmbeddingsMap.TryGetValue(fileChunkGroup.File.RelativePath, out List<IList<double>>? value))
                {
                    value = new List<IList<double>>();
                    fileEmbeddingsMap[fileChunkGroup.File.RelativePath] = value;
                }

                value.AddRange(fileEmbeddings);
            }

            totalTokens += embeddingResult.TotalTokensCount;
            totalCost += embeddingResult.TotalCost;
        }

        // Merge and create final embeddings for each file
        var codeEmbeddings = new List<CodeEmbedding>();
        foreach (var entry in fileEmbeddingsMap)
        {
            var filePath = entry.Key;
            var embeddings = entry.Value;

            // Merge embeddings for the file
            var mergedEmbedding = MergeEmbeddings(embeddings);

            // Retrieve the original file details from codeFilesMap
            var fileDetails = codeFilesMap.First(file => file.RelativePath == filePath);

            codeEmbeddings.Add(
                new CodeEmbedding
                {
                    RelativeFilePath = fileDetails.RelativePath,
                    TreeSitterFullCode = fileDetails.TreeSitterFullCode,
                    TreeOriginalCode = fileDetails.TreeOriginalCode,
                    Code = fileDetails.OriginalCode,
                    SessionId = chatSession.SessionId,
                    Embeddings = mergedEmbedding,
                }
            );
        }

        await codeEmbeddingsRepository.AddOrUpdateCodeEmbeddings(codeEmbeddings).ConfigureAwait(false);

        return new AddEmbeddingsForFilesResult(totalTokens, totalCost);
    }

    public async Task<GetRelatedEmbeddingsResult> GetRelatedEmbeddings(string userQuery, ChatSession chatSession)
    {
        // Generate embedding for user input based on LLM apis
        var embeddingsResult = await GenerateEmbeddingForUserInput(userQuery).ConfigureAwait(false);

        // Find relevant code based on the user query
        var relevantCodes = codeEmbeddingsRepository.Query(
            embeddingsResult.Embeddings.First(),
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
        return await llmClientManager.GetEmbeddingAsync(new List<string> { userInput }, null).ConfigureAwait(false);
    }

    private async Task<List<FileBatch>> BatchFilesByTokenLimitAsync(
        IEnumerable<CodeFileMap> codeFilesMap,
        int maxBatchTokens
    )
    {
        var fileBatches = new List<FileBatch>();
        var currentBatch = new FileBatch();

        foreach (var file in codeFilesMap)
        {
            // Convert the full code to an input string and split into chunks
            var input = promptManager.GetEmbeddingInputString(file.TreeSitterFullCode);
            var chunks = await SplitTextIntoChunksAsync(input, maxTokens: 8192).ConfigureAwait(false);

            var tokenCountTasks = chunks.Select(chunk => tokenizer.GetTokenCount(chunk));
            var tokenCounts = await Task.WhenAll(tokenCountTasks).ConfigureAwait(false);

            // Pair chunks with their token counts
            var chunkWithTokens = chunks.Zip(
                tokenCounts,
                (chunk, tokenCount) => new { Chunk = chunk, TokenCount = tokenCount }
            );

            foreach (var chunkGroup in chunkWithTokens)
            {
                // If adding this chunk would exceed the batch token limit
                if (currentBatch.TotalTokens + chunkGroup.TokenCount > maxBatchTokens && currentBatch.Files.Count > 0)
                {
                    // Finalize the current batch and start a new one
                    fileBatches.Add(currentBatch);
                    currentBatch = new FileBatch();
                }

                // Add this chunk to the current batch
                if (currentBatch.Files.All(f => f.File != file))
                {
                    // If this is the first chunk of this file in the current batch, add a new FileChunkGroup
                    currentBatch.Files.Add(new FileChunkGroup(file, new List<string> { chunkGroup.Chunk }));
                }
                else
                {
                    // Add the chunk to the existing FileChunkGroup for this file
                    var fileGroup = currentBatch.Files.First(f => f.File == file);
                    fileGroup.Chunks.Add(chunkGroup.Chunk);
                }

                currentBatch.TotalTokens += chunkGroup.TokenCount;
            }
        }

        // Add the last batch if it has content
        if (currentBatch.Files.Count > 0)
        {
            fileBatches.Add(currentBatch);
        }

        return fileBatches;
    }

    private async Task<List<string>> SplitTextIntoChunksAsync(string text, int maxTokens)
    {
        var words = text.Split(' ');
        var chunks = new List<string>();
        var currentChunk = new List<string>();

        foreach (var word in words)
        {
            currentChunk.Add(word);

            // Check token count only when the chunk exceeds a certain word threshold
            if (currentChunk.Count % 50 == 0 || currentChunk.Count == words.Length)
            {
                var currentText = string.Join(" ", currentChunk);
                var currentTokenCount = await tokenizer.GetTokenCount(currentText).ConfigureAwait(false);

                if (currentTokenCount > maxTokens)
                {
                    // Ensure the chunk size is within limits
                    while (currentTokenCount > maxTokens && currentChunk.Count > 1)
                    {
                        currentChunk.RemoveAt(currentChunk.Count - 1);
                        currentText = string.Join(" ", currentChunk);
                        currentTokenCount = await tokenizer.GetTokenCount(currentText).ConfigureAwait(false);
                    }

                    // Add the finalized chunk only if it fits the token limit
                    if (currentTokenCount <= maxTokens)
                    {
                        chunks.Add(currentText);
                    }

                    // Start a new chunk with the current word
                    currentChunk.Clear();
                    currentChunk.Add(word);
                }
            }
        }

        // Add the final chunk if it has content and is within the token limit
        if (currentChunk.Count > 0)
        {
            var finalText = string.Join(" ", currentChunk);
            var finalTokenCount = await tokenizer.GetTokenCount(finalText).ConfigureAwait(false);

            if (finalTokenCount <= maxTokens)
            {
                chunks.Add(finalText);
            }
        }

        return chunks;
    }

    private static IList<double> MergeEmbeddings(IList<IList<double>> embeddings)
    {
        if (embeddings == null || embeddings.Count == 0)
            throw new ArgumentException("The embeddings list cannot be null or empty.");

        int dimension = embeddings.First().Count;
        var mergedEmbedding = new double[dimension];

        foreach (var embedding in embeddings)
        {
            if (embedding.Count != dimension)
                throw new InvalidOperationException("All embeddings must have the same dimensionality.");

            for (int i = 0; i < dimension; i++)
            {
                mergedEmbedding[i] += embedding[i];
            }
        }

        // Average the embeddings to unify them into one
        for (int i = 0; i < dimension; i++)
        {
            mergedEmbedding[i] /= embeddings.Count;
        }

        return mergedEmbedding;
    }
}
