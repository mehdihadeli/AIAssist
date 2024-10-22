using AIAssistant.Models;
using Clients.Models;

namespace AIAssistant.Contracts;

public interface IModelsStorageService
{
    void AddChatModel(ChatModelStorage chatModelStorage);
    void AddEmbeddingModel(EmbeddingModelStorage embeddingModelStorage);
    IReadOnlyList<ChatModelStorage> GetAllChatModels();
    IReadOnlyList<EmbeddingModelStorage> GetAllEmbeddingModels();
    EmbeddingModelStorage? GetEmbeddingModelByName(string name);
    ChatModelStorage? GetChatModelByName(string name);
    AIProvider GetAIProviderFromModel(string modelName, ModelType modelType);
}
