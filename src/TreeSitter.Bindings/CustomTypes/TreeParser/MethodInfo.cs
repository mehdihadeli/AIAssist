namespace TreeSitter.Bindings.CustomTypes.TreeParser;

public class MethodInfo
{
    public string Name { get; set; } = default!;
    public string ReturnType { get; set; } = default!;
    public string AccessModifier { get; set; } = default!;
    public IList<ParameterInfo> Parameters { get; set; } = new List<ParameterInfo>();
    public IList<LocalVariableInfo> LocalVariables { get; set; } = new List<LocalVariableInfo>();
    public IList<LocalFunctionInfo> LocalFunctions { get; set; } = new List<LocalFunctionInfo>();
    public string Definition { get; set; } = default!;
}

public class ParameterInfo
{
    public string Name { get; set; } = default!;
    public string Type { get; set; } = default!;
}
