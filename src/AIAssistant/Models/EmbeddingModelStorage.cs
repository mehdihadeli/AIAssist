using Clients.Models;

namespace AIAssistant.Models;

public record EmbeddingModelStorage(string ModelName, AIProvider AIProvider, double Threshold);
