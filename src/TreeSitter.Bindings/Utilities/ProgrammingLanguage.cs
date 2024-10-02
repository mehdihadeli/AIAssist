using System.ComponentModel;

namespace TreeSitter.Bindings.Utilities;

public enum ProgrammingLanguage
{
    [Description("Go Programming Language")]
    Go,
    [Description("C# (C-Sharp)")]
    CSharp,
    [Description("Java Language")]
    Java,
    [Description("JavaScript (JS)")]
    JavaScript,
    [Description("Python Programming Language")]
    Python,
    [Description("TypeScript (TS)")]
    TypeScript
}