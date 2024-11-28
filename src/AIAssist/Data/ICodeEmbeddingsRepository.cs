using AIAssist.Models;
using BuildingBlocks.InMemoryVectorDatabase.Contracts;

namespace AIAssist.Data;

public interface ICodeEmbeddingsRepository : IGenericVectorRepository<CodeEmbedding, CodeEmbeddingDocument>;
