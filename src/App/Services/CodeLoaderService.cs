using AIRefactorAssistant.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace AIRefactorAssistant.Services;

public class CodeLoaderService(ILogger<CodeLoaderService> logger)
{
    public IList<CodeEmbedding> LoadApplicationCode(string appRelativePath)
    {
        var rootPath = Directory.GetCurrentDirectory();
        var appPath = Path.Combine(rootPath, appRelativePath);

        logger.LogInformation("AppPath is: {AppPath}", appPath);

        var csFiles = Directory.GetFiles(appPath, "*.cs", SearchOption.AllDirectories);
        var embeddings = new List<CodeEmbedding>();

        foreach (var file in csFiles)
        {
            var relativePath = Path.GetRelativePath(appPath, file);
            logger.LogInformation("App relative path is: {RelativePath}", relativePath);

            var fileContent = File.ReadAllText(file);
            var codeEmbedding = CodeEmbeddingByFile(fileContent, relativePath);

            // Add chunk without embeddings initially
            embeddings.Add(codeEmbedding);
        }

        return embeddings;
    }

    private static CodeEmbedding CodeEmbeddingByFile(string code, string relativeFilePath)
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

        // Create a single chunk for the entire file
        var codeEmbedding = new CodeEmbedding
        {
            ClassName = string.Join(", ", classNames),
            MethodsName = string.Join(", ", methodNames),
            Code = code,
            RelativeFilePath = relativeFilePath,
        };

        return codeEmbedding;
    }
}
