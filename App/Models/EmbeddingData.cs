using Newtonsoft.Json;

namespace AIRefactorAssistant.Models;

class EmbeddingData
{
    [JsonProperty("embedding")]
    public List<double> Embedding { get; set; }
}
