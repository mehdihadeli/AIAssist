using Clients.Models;

namespace AIAssist.Models;

public record PromptInformation(string EmbeddedResourceName, CommandType CommandType, CodeDiffType? DiffType);
