using Clients.Models;

namespace AIAssistant.Models;

public record PromptInformation(string EmbeddedResourceName, CommandType CommandType, CodeDiffType? DiffType);
