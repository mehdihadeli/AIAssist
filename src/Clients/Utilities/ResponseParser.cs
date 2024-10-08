using System.Text.Json;
using System.Text.RegularExpressions;
using Clients.Models;

namespace Clients.Utilities;

/// <summary>
/// A utility class for parsing responses to extract code changes from a specific formatted content string.
/// </summary>
public static class ResponseParser
{
    private static readonly JsonSerializerOptions? _options =
        new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    /// <summary>
    /// Extracts a JSON block from the given content and deserializes it into a <see cref="CodeChangeResponse"/> object.
    /// </summary>
    /// <param name="content">The content string that contains the code changes in a specific format.</param>
    /// <returns>
    /// A <see cref="CodeChangeResponse"/> object containing the extracted code changes if a valid JSON block is found;
    /// otherwise, null if no valid JSON block exists.
    /// </returns>
    public static CodeChangeResponse? GetCodeChangesFromResponse(string content)
    {
        string pattern = @"# Final Result\s*```json\s*([\s\S]*?)\s*```";

        // Perform the regex match
        Match match = Regex.Match(content, pattern);

        if (match.Success)
        {
            string jsonBlock = match.Groups[1].Value;

            CodeChangeResponse? response = JsonSerializer.Deserialize<CodeChangeResponse>(jsonBlock, _options);

            return response;
        }
        else
        {
            return null;
        }
    }
}
