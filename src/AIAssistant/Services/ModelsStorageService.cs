using AIAssistant.Contracts;
using AIAssistant.Models;
using Clients.Models;

namespace AIAssistant.Services;

public class ModelsStorageService : IModelsStorageService
{
    private readonly IList<ChatModelStorage> _chatModels = new List<ChatModelStorage>();
    private readonly IList<EmbeddingModelStorage> _embeddingModels = new List<EmbeddingModelStorage>();

    public void AddChatModel(ChatModelStorage chatModelStorage)
    {
        if (_chatModels.All(x => x.ModelName != chatModelStorage.ModelName))
        {
            _chatModels.Add(chatModelStorage);
        }
    }

    public void AddEmbeddingModel(EmbeddingModelStorage embeddingModelStorage)
    {
        if (_embeddingModels.All(x => x.ModelName != embeddingModelStorage.ModelName))
        {
            _embeddingModels.Add(embeddingModelStorage);
        }
    }

    public IReadOnlyList<ChatModelStorage> GetAllChatModels()
    {
        return _chatModels.ToList().AsReadOnly();
    }

    public IReadOnlyList<EmbeddingModelStorage> GetAllEmbeddingModels()
    {
        return _embeddingModels.ToList().AsReadOnly();
    }

    public EmbeddingModelStorage? GetEmbeddingModelByName(string name)
    {
        return _embeddingModels.SingleOrDefault(x => x.ModelName == name);
    }

    public ChatModelStorage? GetChatModelByName(string name)
    {
        return _chatModels.SingleOrDefault(x => x.ModelName == name);
    }

    public AIProvider GetAIProviderFromModel(string modelName, ModelType modelType)
    {
        if (modelType == ModelType.ChatModel)
        {
            var modelStorage = _chatModels.SingleOrDefault(x => x.ModelName == modelName);

            if (modelStorage is null)
            {
                throw new Exception($"Model {modelName} not found.");
            }

            return modelStorage.AIProvider;
        }

        var embeddingModelStorage = _embeddingModels.SingleOrDefault(x => x.ModelName == modelName);

        if (embeddingModelStorage is null)
        {
            throw new Exception($"Model {modelName} not found.");
        }

        return embeddingModelStorage.AIProvider;
    }
}
