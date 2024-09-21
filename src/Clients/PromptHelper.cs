namespace Clients;

public static class PromptHelper
{
    //     /// <summary>
    //     /// Generates a detailed prompt for both OpenAI and Ollama API.
    //     /// </summary>
    //     /// <param name="codeContext">The code context to analyze.</param>
    //     /// <param name="userQuery">The user's request or query.</param>
    //     /// <returns>A final prompt in string format, ready to be sent to the API.</returns>
    //     public static string GenerateSuggestedCodePrompt(string codeContext, string userQuery)
    //     {
    //         string jsonInstruction =
    //             @"
    //                 ### Expected Output:
    //                 Your response should be in **JSON format** with the following structure:
    //
    //                 {
    //                     'codeChanges': [
    //                         {
    //                             'fileRelativePath': 'YourRelativePathHere',
    //                             'beforeChange': 'The original code before modification.',
    //                             'afterChange': 'The modified or new code.',
    //                             'diffs': [
    //                                 ```diff
    //                                 - old line of code
    //                                 + new line of code
    //                                 ```
    //                             ]
    //                         },
    //                         ...
    //                     ],
    //                     'explanation': 'A clear explanation of why these changes were made, including how they address the user's request and any improvements in functionality or design.'
    //                 }
    //
    //                 #### Sample Diff Format:
    //                 For example, if changes are made to a method, the diff could look like this:
    //                 ```diff
    //                 - public double Divide(double a, double b) {
    //                 -     if (b == 0) throw new DivideByZeroException();
    //                 -     return a / b;
    //                 - }
    //                 + public double Divide(double a, double b) {
    //                 +     if (b == 0) throw new DivideByZeroException('Cannot divide by zero.');
    //                 +     return a / b;
    //                 + }
    //                 ```
    //                 ";
    //
    //         return $@"
    //                 ### Instruction:
    //                 You are an expert code assistant and advisor. Your role is to analyze the provided code snippets and make necessary improvements, extend existing functionality, or add new features based on the user's input. Your suggestions should:
    //
    //                 1. Analyze each code block thoroughly and consider how they interact with each other.
    //                 2. Enhance the functionality of the existing code by optimizing it or addressing any potential issues.
    //                 3. Add new features or extend current functionality as specified by the user.
    //                 4. Follow best coding practices, ensuring that the code is clean, efficient, and well-structured.
    //                 5. Handle any potential edge cases or errors, improving robustness where necessary.
    //                 6. Provide clear and concise explanations for all changes or additions, highlighting how they improve the code or address the user's request.
    //                 7. Include well-documented code snippets, with inline comments explaining the purpose of each modification or new feature.
    //
    //                 ### Code Context:
    //                 The following are blocks of code from the user's application. Please analyze them thoroughly and use them as the context for your response:
    //
    //                 {codeContext}
    //
    //                 ### User Query:
    //                 Based on the user's request, they are asking you to perform the following action:
    //
    //                 '{userQuery}'
    //
    //                 {jsonInstruction}
    //                 ";
    //     }

    public static string GenerateSuggestedCodePrompt(string codeContext, string userQuery)
    {
        return $@"
### Instruction:
- You are an expert code assistant and advisor. Your role is to analyze the provided code snippets and make necessary improvements, extend existing functionality, or add new features based on the user's input. 
- You should consider `Code Context` and all of its content as our application context for your search and suggestion by you as a expert code assistant.
- Your search on the `code context` should be case in-sensitive.

### Code Context:
- consider bellow code blocks as your context to suggestion beside of your suggestion and best practices
- The following are blocks of code from the our application, Please analyze them carefully and consider theme as your context to search and suggestion and use them for your response:
{codeContext}

### Request and Input Query:
Based on the request and input query to search and suggestion on application context, we are asking you to perform the following action based on application context and context that we had in the `### Code Context` section:


'{userQuery}'
";
    }
}
