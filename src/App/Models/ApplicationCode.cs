namespace AIRefactorAssistant.Models;

public record ApplicationCode(
    string Code,
    string RelativePath,
    IList<string> ClassesNameList,
    IList<string> MethodsNameList
)
{
    public string ClassesName => string.Join(", ", ClassesNameList);
    public string MethodsName => string.Join(", ", MethodsNameList);
};
