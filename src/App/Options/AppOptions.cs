namespace AIRefactorAssistant.Options;

public class AppOptions
{
    public string Model { get; set; } = "llama3.1";
    public string RootPath { get; set; } = default!;
}
