namespace AIRefactorAssistant.Utils;

public static class ArgumentParser
{
    public static IReadOnlyDictionary<string, string> Parse(string[] args)
    {
        var arguments = new Dictionary<string, string>();

        // Loop through args and look for the '--key=value' format
        foreach (var t in args)
        {
            // Split argument by '=' to extract key and value
            string[] argParts = t.Split(new[] { '=' }, 2);

            if (argParts.Length == 2 && argParts[0].StartsWith("--"))
            {
                string key = argParts[0].Substring(2); // Remove the '--'
                string value = argParts[1]; // Value after '='

                // Store the argument and value
                arguments[key] = value;
            }
        }

        return arguments;
    }
}
