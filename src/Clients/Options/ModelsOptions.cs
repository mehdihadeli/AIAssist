using Clients.Models;

namespace Clients.Options;

// The configuration system uses binding and convention-based matching, not JSON deserialization attributes like JsonPropertyName.

/// <summary>
/// Fully qualified model name with AI provider type and '/' prefix
/// </summary>
public class ModelsOptions : Dictionary<string, ModelOption>;
