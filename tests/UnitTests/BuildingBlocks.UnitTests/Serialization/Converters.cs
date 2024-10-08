using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Humanizer;

namespace BuildingBlocks.UnitTests.Serialization;

public class RoleTypeConverter : JsonConverter<RoleType>
{
    public override RoleType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Read the string value from the JSON
        string roleValue = reader.GetString() ?? string.Empty;

        // Map the string to the corresponding RoleType enum value
        return roleValue.ToLower(CultureInfo.InvariantCulture) switch
        {
            "system" => RoleType.System,
            "user" => RoleType.User,
            "assistant" => RoleType.Assistant,
            _ => throw new JsonException($"Unknown role type: {roleValue}"),
        };
    }

    public override void Write(Utf8JsonWriter writer, RoleType value, JsonSerializerOptions options)
    {
        // Convert the enum value back to string for serialization
        string roleString = value switch
        {
            RoleType.System => RoleType.System.Humanize(LetterCasing.LowerCase),
            RoleType.User => RoleType.User.Humanize(LetterCasing.LowerCase),
            RoleType.Assistant => RoleType.Assistant.Humanize(LetterCasing.LowerCase),
            _ => throw new NotImplementedException($"Unknown role type: {value}"),
        };

        writer.WriteStringValue(roleString);
    }
}
