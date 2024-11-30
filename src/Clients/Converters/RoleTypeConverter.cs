using System.Text.Json;
using System.Text.Json.Serialization;
using Clients.Models;
using Humanizer;

namespace Clients.Converters;

// we use RoleTypeConverter when Role type particpate in a model and we use the model inside of serialization mechanism nor binding configuration because they are not serialization based
public class RoleTypeConverter : JsonConverter<RoleType>
{
    public override RoleType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Read the string value from JSON and convert it to snake_case
        string roleValue = reader.GetString() ?? string.Empty;
        var snakeCaseRole = roleValue.Underscore();

        // Define snake_case mappings for each enum value
        var system = RoleType.System.ToString().Underscore();
        var user = RoleType.User.ToString().Underscore();
        var assistant = RoleType.Assistant.ToString().Underscore();

        // Convert snake_case string to RoleType enum
        return snakeCaseRole switch
        {
            var role when role == system => RoleType.System,
            var role when role == user => RoleType.User,
            var role when role == assistant => RoleType.Assistant,
            _ => throw new JsonException($"Unknown role type: {roleValue}"),
        };
    }

    public override void Write(Utf8JsonWriter writer, RoleType value, JsonSerializerOptions options)
    {
        // Define snake_case strings for each enum value
        string system = RoleType.System.ToString().Underscore();
        string user = RoleType.User.ToString().Underscore();
        string assistant = RoleType.Assistant.ToString().Underscore();

        // Convert RoleType enum to corresponding snake_case string
        string roleString = value switch
        {
            RoleType.System => system,
            RoleType.User => user,
            RoleType.Assistant => assistant,
            _ => throw new JsonException($"Unknown role type: {value}"),
        };

        writer.WriteStringValue(roleString);
    }
}
