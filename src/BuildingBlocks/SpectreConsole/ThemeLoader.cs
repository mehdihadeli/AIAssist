using System.Reflection;
using System.Text.Json;
using BuildingBlocks.Serialization;
using BuildingBlocks.Utils;

namespace BuildingBlocks.SpectreConsole;

public static class ThemeLoader
{
    public static ColorTheme? LoadTheme(string? theme = "dracula")
    {
        var jsonTheme = FilesUtilities.ReadEmbeddedResource(
            Assembly.GetExecutingAssembly(),
            $"{nameof(BuildingBlocks)}.{nameof(SpectreConsole)}.Themes.{theme ?? "vscode_light"}.json"
        );

        var themeObject = JsonSerializer.Deserialize<ColorTheme>(jsonTheme, JsonObjectSerializer.SnakeCaseOptions);

        return themeObject;
    }
}
