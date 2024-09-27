using AIRefactorAssistant.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace AIRefactorAssistant.Services;

public class CodeLoaderService(ILogger<CodeLoaderService> logger)
{
    public IEnumerable<ApplicationCode> LoadApplicationCodes(string appRelativePath)
    {
        var rootPath = Directory.GetCurrentDirectory();
        var appPath = Path.Combine(rootPath, appRelativePath);

        logger.LogInformation("AppPath is: {AppPath}", appPath);

        var csFiles = Directory.GetFiles(appPath, "*.cs", SearchOption.AllDirectories);
        var applicationCodes = new List<ApplicationCode>();

        foreach (var file in csFiles)
        {
            var relativePath = Path.GetRelativePath(appPath, file);
            logger.LogInformation("App relative path is: {RelativePath}", relativePath);

            var fileContent = File.ReadAllText(file);

            applicationCodes.Add(CreateApplicationCode(fileContent, relativePath));
        }

        return applicationCodes;
    }

    private static ApplicationCode CreateApplicationCode(string code, string relativeFilePath)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = syntaxTree.GetRoot();

        var classNames = new List<string>();
        var methodNames = new List<string>();

        // Extract all class names
        var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        foreach (var classDeclaration in classDeclarations)
        {
            classNames.Add(classDeclaration.Identifier.Text);

            // Extract all method names within the class
            var methodDeclarations = classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var methodDeclaration in methodDeclarations)
            {
                methodNames.Add(methodDeclaration.Identifier.Text);
            }
        }

        var applicationCode = new ApplicationCode(code, relativeFilePath, classNames, methodNames);

        return applicationCode;
    }
}
