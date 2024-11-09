using AIAssistant.Models;
using BuildingBlocks.InMemoryVectorDatabase.Contracts;

namespace AIAssistant.Data;

public interface ICodeEmbeddingsRepository : IGenericVectorRepository<CodeEmbedding, CodeEmbeddingDocument>;
