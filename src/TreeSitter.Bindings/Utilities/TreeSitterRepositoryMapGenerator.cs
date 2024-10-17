using System.Text;
using BuildingBlocks.Extensions;
using TreeSitter.Bindings.CustomTypes.TreeParser;
using static TreeSitter.Bindings.TSBindingsParser;
using static TreeSitter.Bindings.Utilities.TreeSitterParser;
using FileInfo = TreeSitter.Bindings.CustomTypes.TreeParser.FileInfo;

namespace TreeSitter.Bindings.Utilities;

// ref: https://tree-sitter.github.io/tree-sitter/using-parsers

public static class TreeSitterRepositoryMapGenerator
{
    /// <summary>
    /// Generate short tree-sitter map based on supported languages. If language is not supported, it will return the code.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="path"></param>
    /// <param name="repositoryMap"></param>
    /// <param name="writeFullTree"></param>
    /// <returns></returns>
    public static string GenerateTreeSitterRepositoryMap(
        string code,
        string path,
        RepositoryMap repositoryMap,
        bool writeFullTree = false
    )
    {
        var language = path.GetLanguageFromFilePath();
        if (language is null)
            return code;

        unsafe
        {
            var parser = GetParser(language.Value);
            var tree = GetCodeTree(parser, code);
            var defaultQuery = GetLanguageDefaultQuery(language.Value);

            var rootNode = GetRootNode(tree);
            var queryCursor = query_cursor_new();
            query_cursor_exec(queryCursor, defaultQuery, rootNode);

            TSQueryMatch match;
            var fileInfo = new FileInfo { Path = path };

            // Convert code to byte array for extracting matched code using byte positions
            byte[] byteArrayCode = Encoding.UTF8.GetBytes(code);

            // ReSharper disable once TooWideLocalVariableScope
            // declaration of namespace should be outside of while loop
            NamespaceInfo? currentNamespace = null;

            while (query_cursor_next_match(queryCursor, &match))
            {
                // Dictionary to group captures by their names
                var captureTags = new Dictionary<string, IList<TSNode>>();

                // Populate the dictionary by capture Name
                for (int i = 0; i < match.capture_count; i++)
                {
                    var capture = match.captures[i];
                    var node = capture.node;

                    uint length = 0;

                    sbyte* captureNamePtr = query_capture_name_for_id(defaultQuery, capture.index, &length);

                    string captureName = new GeneratedCString(captureNamePtr);

                    if (!captureTags.ContainsKey(captureName))
                        captureTags.Add(captureName, [node]);
                    else
                    {
                        var captureValues = captureTags.GetValueOrDefault(captureName);
                        captureValues?.Add(node);
                    }
                }

                AddNamespace(captureTags, byteArrayCode, fileInfo, ref currentNamespace);

                AddTopLevelStatement(captureTags, byteArrayCode, fileInfo);

                // Try to create or update a ClassInfo object and populate its fields based on captured data
                AddClass(captureTags, byteArrayCode, fileInfo, currentNamespace);

                // Try to create or update a RecordInfo object and populate its fields based on captured data
                AddRecord(captureTags, byteArrayCode, fileInfo, currentNamespace);

                // Try to create or update a StructInfo object and populate its fields based on captured data
                AddStruct(captureTags, byteArrayCode, fileInfo, currentNamespace);

                // Try to create or update a InterfaceInfo object and populate its fields based on captured data
                AddInterface(captureTags, byteArrayCode, fileInfo, currentNamespace);

                AddEnum(captureTags, byteArrayCode, fileInfo, currentNamespace);
            }

            // cleanup resources
            query_cursor_delete(queryCursor);
            query_delete(defaultQuery);
            tree_delete(tree);
            parser_delete(parser);

            AddFileToRepositoryMap(repositoryMap, fileInfo);
        }

        return GenerateTreeString(repositoryMap, applicationName: "root", writeFullTree: writeFullTree);
    }

    private static void AddNamespace(
        Dictionary<string, IList<TSNode>> captureTags,
        byte[] byteArrayCode,
        FileInfo fileInfo,
        ref NamespaceInfo? currentNamespace
    )
    {
        if (
            captureTags.TryGetValue(Constants.NamespaceCaptureTags.Name, out var namespaceNameNodes)
            && captureTags.TryGetValue(Constants.NamespaceCaptureTags.Definition, out var namespaceDefinitionNodes)
        )
        {
            var name = GetMatchedCode(byteArrayCode, namespaceNameNodes.First());
            var definition = GetMatchedCode(byteArrayCode, namespaceDefinitionNodes.First());

            if (!fileInfo.Namespaces.Any(x => x.Name == name && x.Definition == definition))
            {
                var ns = new NamespaceInfo { Definition = definition, Name = name };
                fileInfo.Namespaces.Add(ns);

                currentNamespace = ns;
            }
            else
            {
                var ns = fileInfo.Namespaces.SingleOrDefault(x => x.Name == name && x.Definition == definition);

                if (ns is not null)
                {
                    currentNamespace = ns;
                }
            }
        }

        if (
            captureTags.TryGetValue(Constants.FileScopedNamespaceCaptureTags.Name, out var namespaceScopedNameNodes)
            && captureTags.TryGetValue(
                Constants.FileScopedNamespaceCaptureTags.Definition,
                out var namespaceScopedDefinitionNodes
            )
        )
        {
            var name = GetMatchedCode(byteArrayCode, namespaceScopedNameNodes.First());
            var definition = GetMatchedCode(byteArrayCode, namespaceScopedDefinitionNodes.First());

            if (!fileInfo.Namespaces.Any(x => x.Name == name && x.Definition == definition))
            {
                var ns = new NamespaceInfo { Definition = definition, Name = name };
                fileInfo.Namespaces.Add(ns);

                currentNamespace = ns;
            }
            else
            {
                var ns = fileInfo.Namespaces.SingleOrDefault(x => x.Name == name && x.Definition == definition);

                if (ns is not null)
                {
                    currentNamespace = ns;
                }
            }
        }
    }

    private static void AddTopLevelStatement(
        Dictionary<string, IList<TSNode>> captureTags,
        byte[] byteArrayCode,
        FileInfo fileInfo
    )
    {
        if (
            captureTags.TryGetValue(
                Constants.TopLevelStatementCaptureTags.Definition,
                out var topLevelStatementDefinitionNodes
            )
        )
        {
            var definition = GetMatchedCode(byteArrayCode, topLevelStatementDefinitionNodes.First());

            if (fileInfo.TopLevelStatementsDefinition.All(x => x != definition))
                fileInfo.TopLevelStatementsDefinition.Add(definition);
        }
    }

    private static void AddStruct(
        Dictionary<string, IList<TSNode>> captureTags,
        byte[] byteArrayCode,
        FileInfo fileInfo,
        NamespaceInfo? currentNamespace
    )
    {
        if (
            captureTags.TryGetValue(Constants.StructCaptureTags.Name, out var structNameNodes)
            && captureTags.TryGetValue(Constants.StructCaptureTags.Definition, out var structDefinitionNodes)
        )
        {
            StructInfo? currentStruct;

            var name = GetMatchedCode(byteArrayCode, structNameNodes.First());
            var definition = GetMatchedCode(byteArrayCode, structDefinitionNodes.First());

            if (currentNamespace != null)
            {
                var strc = currentNamespace.Structs.SingleOrDefault(x => x.Name == name && x.Definition == definition);

                if (strc is null)
                {
                    currentNamespace.Structs.Add(
                        currentStruct = new StructInfo
                        {
                            // we have just one structNameNodes in each iteration
                            Name = name,
                            Definition = definition,
                        }
                    );
                }
                else
                    currentStruct = strc;
            }
            else
            {
                var strc = fileInfo.Structs.SingleOrDefault(x => x.Name == name && x.Definition == definition);

                if (strc is null)
                {
                    fileInfo.Structs.Add(
                        currentStruct = new StructInfo
                        {
                            // we have just one structNameNodes in each iteration
                            Name = name,
                            Definition = definition,
                        }
                    );
                }
                else
                {
                    currentStruct = strc;
                }
            }

            // add fields to the record
            FindOrAddFields(currentStruct, byteArrayCode, captureTags);

            // add comment to the record
            FindOrAddComment(currentStruct, byteArrayCode, captureTags);

            // Add properties to the record
            FindOrAddProperties(currentStruct, byteArrayCode, captureTags);

            // Add methods to the record
            FindOrAddMethod(currentStruct, byteArrayCode, captureTags);
        }
    }

    private static void AddInterface(
        Dictionary<string, IList<TSNode>> captureTags,
        byte[] byteArrayCode,
        FileInfo fileInfo,
        NamespaceInfo? currentNamespace
    )
    {
        if (
            captureTags.TryGetValue(Constants.InterfaceCaptureTags.Name, out var interfaceNameNodes)
            && captureTags.TryGetValue(Constants.InterfaceCaptureTags.Definition, out var interfaceDefinitionNodes)
        )
        {
            InterfaceInfo? currentInterface;

            var name = GetMatchedCode(byteArrayCode, interfaceNameNodes.First());
            var definition = GetMatchedCode(byteArrayCode, interfaceDefinitionNodes.First());

            if (currentNamespace != null)
            {
                var interfac = currentNamespace.Interfaces.SingleOrDefault(x =>
                    x.Name == name && x.Definition == definition
                );

                if (interfac is null)
                {
                    currentNamespace.Interfaces.Add(
                        currentInterface = new InterfaceInfo
                        {
                            // we have just one interfaceNameNodes in each iteration
                            Name = name,
                            Definition = definition,
                        }
                    );
                }
                else
                    currentInterface = interfac;
            }
            else
            {
                var interfac = fileInfo.Interfaces.SingleOrDefault(x => x.Name == name && x.Definition == definition);

                if (interfac is null)
                {
                    fileInfo.Interfaces.Add(
                        currentInterface = new InterfaceInfo
                        {
                            // we have just one interfaceNameNodes in each iteration
                            Name = name,
                            Definition = definition,
                        }
                    );
                }
                else
                {
                    currentInterface = interfac;
                }
            }

            // add comment to the record
            FindOrAddComment(currentInterface, byteArrayCode, captureTags);

            // Add properties to the record
            FindOrAddProperties(currentInterface, byteArrayCode, captureTags);

            // Add methods to the record
            FindOrAddMethod(currentInterface, byteArrayCode, captureTags);
        }
    }

    private static void AddRecord(
        Dictionary<string, IList<TSNode>> captureTags,
        byte[] byteArrayCode,
        FileInfo fileInfo,
        NamespaceInfo? currentNamespace
    )
    {
        if (
            captureTags.TryGetValue(Constants.RecordCaptureTags.Name, out var recordNameNodes)
            && captureTags.TryGetValue(Constants.RecordCaptureTags.Definition, out var recordDefinitionNodes)
        )
        {
            RecordInfo? currentRecord;

            var name = GetMatchedCode(byteArrayCode, recordNameNodes.First());
            var definition = GetMatchedCode(byteArrayCode, recordDefinitionNodes.First());

            if (currentNamespace != null)
            {
                var rec = currentNamespace.Records.SingleOrDefault(x => x.Name == name && x.Definition == definition);

                if (rec is null)
                {
                    currentNamespace.Records.Add(
                        currentRecord = new RecordInfo
                        {
                            // we have just one recordNameNodes in each iteration
                            Name = name,
                            Definition = definition,
                        }
                    );
                }
                else
                    currentRecord = rec;
            }
            else
            {
                var rec = fileInfo.Records.SingleOrDefault(x => x.Name == name && x.Definition == definition);

                if (rec is null)
                {
                    fileInfo.Records.Add(
                        currentRecord = new RecordInfo
                        {
                            // we have just one recordNameNodes in each iteration
                            Name = name,
                            Definition = definition,
                        }
                    );
                }
                else
                {
                    currentRecord = rec;
                }
            }

            // add fields to the record
            FindOrAddFields(currentRecord, byteArrayCode, captureTags);

            // add comment to the record
            FindOrAddComment(currentRecord, byteArrayCode, captureTags);

            // Add properties to the record
            FindOrAddProperties(currentRecord, byteArrayCode, captureTags);

            // Add methods to the record
            FindOrAddMethod(currentRecord, byteArrayCode, captureTags);
        }
    }

    private static void AddClass(
        Dictionary<string, IList<TSNode>> captureTags,
        byte[] byteArrayCode,
        FileInfo fileInfo,
        NamespaceInfo? currentNamespace
    )
    {
        if (
            captureTags.TryGetValue(Constants.ClassCaptureTags.Name, out var classNameNodes)
            && captureTags.TryGetValue(Constants.ClassCaptureTags.Definition, out var classDefinitionNodes)
        )
        {
            ClassInfo? currentClass;

            var name = GetMatchedCode(byteArrayCode, classNameNodes.First());
            var definition = GetMatchedCode(byteArrayCode, classDefinitionNodes.First());

            if (currentNamespace != null)
            {
                var cls = currentNamespace.Classes.SingleOrDefault(x => x.Name == name && x.Definition == definition);

                if (cls is null)
                {
                    currentNamespace.Classes.Add(
                        currentClass = new ClassInfo
                        {
                            // we have just one classNameNodes in each iteration
                            Name = name,
                            Definition = definition,
                        }
                    );
                }
                else
                    currentClass = cls;
            }
            else
            {
                var cls = fileInfo.Classes.SingleOrDefault(x => x.Name == name && x.Definition == definition);

                if (cls is null)
                {
                    fileInfo.Classes.Add(
                        currentClass = new ClassInfo
                        {
                            // we have just one classNameNodes in each iteration
                            Name = name,
                            Definition = definition,
                        }
                    );
                }
                else
                {
                    currentClass = cls;
                }
            }

            // add fields to the class
            FindOrAddFields(currentClass, byteArrayCode, captureTags);

            // add comment to the class
            FindOrAddComment(currentClass, byteArrayCode, captureTags);

            // Add properties to the class
            FindOrAddProperties(currentClass, byteArrayCode, captureTags);

            // Add methods to the class
            FindOrAddMethod(currentClass, byteArrayCode, captureTags);
        }
    }

    private static void AddEnum(
        Dictionary<string, IList<TSNode>> captureTags,
        byte[] byteArrayCode,
        FileInfo fileInfo,
        NamespaceInfo? currentNamespace
    )
    {
        if (
            captureTags.TryGetValue("name.enum", out var enumNameNodes)
            && captureTags.TryGetValue("definition.enum", out var enumDefinitionNodes)
        )
        {
            var name = GetMatchedCode(byteArrayCode, enumNameNodes.First());
            var definition = GetMatchedCode(byteArrayCode, enumDefinitionNodes.First());

            // resting `currentEnum` here for the grammar query
            var currentEnum = new EnumInfo
            {
                // we have just one enumNameNodes in each iteration
                Name = name,
                Definition = definition,
            };

            if (currentNamespace != null)
            {
                if (!currentNamespace.Enums.Any(x => x.Name == name && x.Definition == definition))
                    currentNamespace.Enums.Add(currentEnum);
            }
            else
            {
                if (!fileInfo.Enums.Any(x => x.Name == name && x.Definition == definition))
                    fileInfo.Enums.Add(currentEnum);
            }
        }

        if (
            captureTags.TryGetValue(Constants.EnumMemberCaptureTags.Name, out var enumMemberNameNodes)
            && captureTags.TryGetValue(Constants.EnumMemberCaptureTags.EnumName, out var enumNameMemberNodes)
        )
        {
            var member = GetMatchedCode(byteArrayCode, enumMemberNameNodes.First());
            var enumName = GetMatchedCode(byteArrayCode, enumNameMemberNodes.First());

            if (currentNamespace != null)
            {
                var en = currentNamespace.Enums.SingleOrDefault(x => x.Name == enumName);
                en?.Members.Add(member);
            }
            else
            {
                var en = fileInfo.Enums.SingleOrDefault(x => x.Name == enumName);
                en?.Members.Add(member);
            }
        }
    }

    static IEnumerable<MethodInfo> FindOrAddMethod(
        IMethodElement? methodElement,
        byte[] byteArrayCode,
        IDictionary<string, IList<TSNode>> captureTags
    )
    {
        if (methodElement is null)
            return new List<MethodInfo>();

        if (
            captureTags.TryGetValue(Constants.MethodCaptureTags.Name, out var methodNameNodes)
            && captureTags.TryGetValue(Constants.MethodCaptureTags.Return, out var methodReturnNodes)
            && captureTags.TryGetValue(Constants.MethodCaptureTags.Definition, out var methodDefinitionNodes)
        )
        {
            captureTags.TryGetValue(Constants.MethodCaptureTags.Modifier, out var methodModifierNodes);

            var methodName = GetMatchedCode(byteArrayCode, methodNameNodes.First());
            var methodReturn = GetMatchedCode(byteArrayCode, methodReturnNodes.First());
            var methodDefinition = GetMatchedCode(byteArrayCode, methodDefinitionNodes.First());
            var methodModifier =
                methodModifierNodes == null
                    ? string.Empty
                    : GetMatchedCode(byteArrayCode, methodModifierNodes.FirstOrDefault());

            var method = methodElement.Methods.SingleOrDefault(x =>
                x.Name == methodName
                && x.ReturnType == methodReturn
                && x.AccessModifier == methodModifier
                && x.Definition == methodDefinition
            );

            if (method is null)
            {
                method = new MethodInfo
                {
                    Name = methodName,
                    ReturnType = methodReturn,
                    AccessModifier = methodModifier,
                    Definition = methodDefinition,
                };
                methodElement.Methods.Add(method);
            }

            // Capture method parameters if the method is found
            if (
                captureTags.TryGetValue(Constants.MethodParameterCaptureTags.ParameterName, out var parameterNameNodes)
                && captureTags.TryGetValue(
                    Constants.MethodParameterCaptureTags.ParameterType,
                    out var parameterTypeNodes
                )
            )
            {
                var parameterType = GetMatchedCode(byteArrayCode, parameterTypeNodes.First());
                var parameterName = GetMatchedCode(byteArrayCode, parameterNameNodes.First());
                method.Parameters.Add(new ParameterInfo { Name = parameterName, Type = parameterType });
            }

            // add comment to the method
            if (
                captureTags.TryGetValue(Constants.MethodCaptureTags.Comment, out var commentNodes)
                && string.IsNullOrEmpty(method.Comments)
            )
            {
                foreach (var comment in commentNodes)
                {
                    method.Comments += GetMatchedCode(byteArrayCode, comment) + Environment.NewLine;
                }
            }
        }

        return methodElement.Methods;
    }

    static string FindOrAddComment(
        ICommentElement? commentElement,
        byte[] byteArrayCode,
        IDictionary<string, IList<TSNode>> captureTags
    )
    {
        if (commentElement is null)
            return string.Empty;

        if (
            captureTags.TryGetValue("comment.class", out var commentNodes)
            && string.IsNullOrEmpty(commentElement.Comments)
        )
        {
            foreach (var comment in commentNodes)
            {
                commentElement.Comments += GetMatchedCode(byteArrayCode, comment) + Environment.NewLine;
            }
        }

        return commentElement.Comments;
    }

    static IEnumerable<FieldInfo> FindOrAddFields(
        IFieldElement? fieldElement,
        byte[] byteArrayCode,
        IDictionary<string, IList<TSNode>> captureTags
    )
    {
        if (fieldElement is null)
            return new List<FieldInfo>();

        if (
            captureTags.TryGetValue("name.field", out var fieldNameNodes)
            && captureTags.TryGetValue("type.field", out var fieldTypeNodes)
            && captureTags.TryGetValue("definition.field", out var fieldDefinitionNodes)
        )
        {
            for (int i = 0; i < fieldNameNodes.Count; i++)
            {
                var filedName = GetMatchedCode(byteArrayCode, fieldNameNodes[i]);
                var filedType = GetMatchedCode(byteArrayCode, fieldTypeNodes[i]);
                var filedDefinition = GetMatchedCode(byteArrayCode, fieldDefinitionNodes[i]);

                // Try to find an existing field with the same name, type, and definition
                var field = fieldElement.Fields.SingleOrDefault(x =>
                    x.Name == filedName && x.Type == filedType && x.Definition == filedDefinition
                );

                // If the field doesn't exist, create a new one and add it to the class
                if (field is null)
                {
                    field = new FieldInfo
                    {
                        Name = filedName,
                        Type = filedType,
                        Definition = filedDefinition,
                    };
                    fieldElement.Fields.Add(field);
                }
            }
        }

        return fieldElement.Fields;
    }

    static IEnumerable<PropertyInfo> FindOrAddProperties(
        IPropertyElement? propertyElement,
        byte[] byteArrayCode,
        IDictionary<string, IList<TSNode>> captureTags
    )
    {
        if (propertyElement is null)
            return new List<PropertyInfo>();

        if (
            captureTags.TryGetValue("name.property", out var propertyNameNodes)
            && captureTags.TryGetValue("type.property", out var propertyTypeNodes)
            && captureTags.TryGetValue("modifier.property", out var propertyModifierNodes)
            && captureTags.TryGetValue("definition.property", out var propertyDefinitionNodes)
        )
        {
            for (int i = 0; i < propertyNameNodes.Count; i++)
            {
                var name = GetMatchedCode(byteArrayCode, propertyNameNodes[i]);
                var type = GetMatchedCode(byteArrayCode, propertyTypeNodes[i]);
                var accessModifier = GetMatchedCode(byteArrayCode, propertyModifierNodes[i]);
                var definition = GetMatchedCode(byteArrayCode, propertyDefinitionNodes[i]);

                // Try to find an existing property with the same name, type, and definition
                var propertyInfo = propertyElement.Properties.SingleOrDefault(x =>
                    x.Name == name && x.Type == type && x.AccessModifier == accessModifier && x.Definition == definition
                );

                // If the property doesn't exist, create a new one and add it to the class
                if (propertyInfo is null)
                {
                    propertyInfo = new PropertyInfo
                    {
                        Name = name,
                        Type = type,
                        AccessModifier = accessModifier,
                        Definition = definition,
                        // Optionally set other properties here
                    };
                    propertyElement.Properties.Add(propertyInfo);
                }
            }
        }

        return propertyElement.Properties;
    }

    private delegate void CaptureHandler(string captureTagKey, string captureValue);

    private static void ProcessCaptureTags(
        IDictionary<string, IList<TSNode>> captureTags,
        byte[] byteArrayCode,
        CaptureHandler handleCapture
    )
    {
        foreach (var (captureTagKey, captureNodesTagValues) in captureTags)
        {
            foreach (var captureNodeTagValue in captureNodesTagValues)
            {
                var captureValue = GetMatchedCode(byteArrayCode, captureNodeTagValue);
                handleCapture(captureTagKey, captureValue);
            }
        }
    }

    private static string GetMatchedCode(byte[] byteArrayCode, TSNode node)
    {
        var startByte = node_start_byte(node);
        var endByte = node_end_byte(node);

        // Fetch the matched code from the byte array based on start and end byte positions
        var matchedCode = Encoding.UTF8.GetString(byteArrayCode, (int)startByte, (int)(endByte - startByte));

        // Trim the result to remove any leading or trailing whitespace
        return matchedCode.Trim();
    }

    private static string GenerateTreeString(
        RepositoryMap repositoryMap,
        string applicationName = "root",
        bool writeFullTree = false
    )
    {
        var stringBuilder = new StringBuilder();

        // Add the application Name at the root of the tree
        stringBuilder.AppendLine($"{applicationName}/");

        // Keep track of directories and maintain folder structure
        var directories = new Dictionary<string, bool>();

        foreach (var file in repositoryMap.Files)
        {
            // Process the file path, splitting it into directories and file Name
            var filePathParts = file.Path.Split('/');
            var directoryPath = string.Join("/", filePathParts.Take(filePathParts.Length - 1));
            var fileName = filePathParts.Last();

            // Ensure the directory structure is preserved
            if (!string.IsNullOrEmpty(directoryPath) && !directories.ContainsKey(directoryPath))
            {
                AppendDirectory(stringBuilder, directoryPath);
                directories[directoryPath] = true;
            }

            // Append the file header (e.g., Add.cs:)
            AppendFileHeader(stringBuilder, fileName, directoryPath);

            // Adjust the indentation based on the directory structure
            var indent = directoryPath != "" ? "│   │   " : "│   ";

            // Append full or summary file content
            if (writeFullTree)
            {
                AppendFileFull(stringBuilder, file, indent);
            }
            else
            {
                AppendFileSummary(stringBuilder, file, indent);
            }
        }

        return stringBuilder.ToString();
    }

    // Appends a directory in the folder structure
    private static void AppendDirectory(StringBuilder sb, string directoryPath)
    {
        var folders = directoryPath.Split('/');
        var indent = "";

        foreach (var folder in folders)
        {
            sb.AppendLine($"{indent}├── {folder}/");
            indent += "│   ";
        }
    }

    // Appends a file header with the proper indentation and directory structure
    private static void AppendFileHeader(StringBuilder sb, string fileName, string directoryPath)
    {
        var indent = string.IsNullOrEmpty(directoryPath) ? "├── " : "│   ├── ";
        sb.AppendLine($"{indent}{fileName}:");
    }

    private static void AppendFileContent(StringBuilder sb, FileInfo file, string indent, bool isFullTree)
    {
        // Generate namespaces and their members
        foreach (var ns in file.Namespaces)
        {
            if (isFullTree)
                AppendNamespaceFull(sb, ns, indent);
            else
                AppendNamespaceSummary(sb, ns, indent);
        }

        // Generate top-level statement in the `FileInfo` which is in top-level and without any namespace
        if (file.TopLevelStatementsDefinition.Any())
        {
            if (isFullTree)
            {
                AppendTopLevelStatementFull(sb, file, indent);
            }
            else
            {
                // Generate the summary of the top-level statement (only the first line)
                sb.AppendLine(
                    $"{
                        indent
                    }├── Top-level statement: {
                        file.TopLevelStatementsDefinition.First()
                    }"
                );
            }
        }

        // Generate top-level `classes` in the FileInfo which is inside of top-level and without any namespace
        AppendClasses(sb, file.Classes, indent, isFullTree);
        // Generate top-level `enums` in the FileInfo which is inside of top-level and without any namespace
        AppendEnums(sb, file.Enums, indent, isFullTree);
        // Generate top-level `structs` in the FileInfo which is inside of top-level and without any namespace
        AppendStructs(sb, file.Structs, indent, isFullTree);
        // Generate top-level `records` in the FileInfo which is inside of top-level and without any namespace
        AppendRecords(sb, file.Records, indent, isFullTree);
        // Generate top-level `interfaces` in the FileInfo which is inside of top-level and without any namespace
        AppendInterfaces(sb, file.Interfaces, indent, isFullTree);
    }

    // AppendFileFull method (detailed mode)
    private static void AppendFileFull(StringBuilder sb, FileInfo file, string indent)
    {
        AppendFileContent(sb, file, indent, true);
    }

    // AppendFileSummary method (summary mode)
    private static void AppendFileSummary(StringBuilder sb, FileInfo file, string indent)
    {
        AppendFileContent(sb, file, indent, false);
    }

    private static void AppendNamespaceSummary(StringBuilder sb, NamespaceInfo ns, string indent)
    {
        // Append the namespace header with a colon
        sb.AppendLine($"{indent}├── namespace: {ns.Name}");
        indent += "│   "; // Indent for members within the namespace

        // Iterate through classes in the namespace
        foreach (var cls in ns.Classes)
        {
            AppendClassSummary(sb, cls, indent);
        }

        // Iterate through enums in the namespace
        foreach (var enm in ns.Enums)
        {
            AppendEnumSummary(sb, enm, indent);
        }

        // Iterate through records in the namespace
        foreach (var rec in ns.Records)
        {
            AppendRecordSummary(sb, rec, indent);
        }

        // Iterate through interfaces in the namespace
        foreach (var iface in ns.Interfaces)
        {
            AppendInterfaceSummary(sb, iface, indent);
        }

        // Iterate through structs in the namespace
        foreach (var strct in ns.Structs)
        {
            AppendStructSummary(sb, strct, indent);
        }
    }

    private static void AppendClassSummary(StringBuilder sb, ClassInfo cls, string indent)
    {
        sb.AppendLine($"{indent}├── class: {cls.Name}");

        var innerIndent = indent + "│   ";

        // Append class members (fields, properties, methods, etc.)
        foreach (var property in cls.Properties)
        {
            sb.AppendLine($"{innerIndent}├── property: {property.Definition}");
        }

        foreach (var method in cls.Methods)
        {
            AppendMethodSummary(sb, method, $"{innerIndent}├── method: ");
        }

        foreach (var field in cls.Fields)
        {
            sb.AppendLine($"{innerIndent}├── field: {field.Definition}");
        }
    }

    private static void AppendEnumSummary(StringBuilder sb, EnumInfo enm, string indent)
    {
        string singleLineCode = string.Join(
            " ",
            enm.Definition.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries) // Split by new lines
                .Select(line => line.Trim()) // Trim each line
                .Where(line => !string.IsNullOrWhiteSpace(line)) // Remove empty lines
        );

        sb.AppendLine($"{indent}├── enum: {singleLineCode}");
    }

    private static void AppendRecordSummary(StringBuilder sb, RecordInfo rec, string indent)
    {
        sb.AppendLine($"{indent}├── record: {rec.Name}");

        var innerIndent = indent + "│   ";

        // Append record members
        foreach (var property in rec.Properties)
        {
            sb.AppendLine($"{innerIndent}├── property: {property.Definition}");
        }

        foreach (var method in rec.Methods)
        {
            AppendMethodSummary(sb, method, $"{innerIndent}├── method: ");
        }

        foreach (var field in rec.Fields)
        {
            sb.AppendLine($"{innerIndent}├── field: {field.Definition}");
        }
    }

    private static void AppendInterfaceSummary(StringBuilder sb, InterfaceInfo iface, string indent)
    {
        sb.AppendLine($"{indent}├── interface: {iface.Name}");

        var innerIndent = indent + "│   ";

        // Append interface members
        foreach (var method in iface.Methods)
        {
            AppendMethodSummary(sb, method, $"{innerIndent}├── method: ");
        }

        foreach (var property in iface.Properties)
        {
            sb.AppendLine($"{innerIndent}├── property: {property.Definition}");
        }
    }

    private static void AppendStructSummary(StringBuilder sb, StructInfo strct, string indent)
    {
        sb.AppendLine($"{indent}├── struct: {strct.Name}");

        var innerIndent = indent + "│   ";

        // Append struct members
        foreach (var property in strct.Properties)
        {
            sb.AppendLine($"{innerIndent}├── property: {property.Definition}");
        }

        foreach (var method in strct.Methods)
        {
            AppendMethodSummary(sb, method, $"{innerIndent}├── method: ");
        }

        foreach (var field in strct.Fields)
        {
            sb.AppendLine($"{innerIndent}├── field: {field.Definition}");
        }
    }

    private static void AppendMethodSummary(StringBuilder sb, MethodInfo method, string prefix)
    {
        // Get the parameter list as a string
        var parameterList = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));

        // Append the access modifier, return type, method Name, and parameter list to the StringBuilder
        sb.AppendLine($"{prefix}{method.AccessModifier} {method.ReturnType} {method.Name}({parameterList});");
    }

    private static void AppendTopLevelStatementFull(StringBuilder sb, FileInfo file, string indent)
    {
        var statement = string.Join(Environment.NewLine, file.TopLevelStatementsDefinition);

        // Output the class header
        sb.AppendLine($"{indent}├── Top-level statement: ");
        var innerIndent = indent + "│   ";
        var indentedTopLevel = IndentDefinitionBlock(statement, innerIndent);
        sb.Append(indentedTopLevel);
    }

    private static void AppendNamespaceFull(StringBuilder sb, NamespaceInfo ns, string indent)
    {
        // we don't use `ns.Definition`. the problem is when we use namespace definition and if our namespace is a scoped namespace we can't get namespace content with capturing namespace definition, but it works correctly and give us namespace content in blocked namespace
        // https://github.com/tree-sitter/tree-sitter-c-sharp/issues/338
        sb.AppendLine($"{indent}├── namespace: {ns.Name};");

        var innerIndent = indent + "│   ";

        // Append classes, enums, records, interfaces, and structs inside the namespace
        foreach (var cls in ns.Classes)
        {
            AppendClassFull(sb, cls, innerIndent);
        }

        foreach (var enm in ns.Enums)
        {
            AppendEnumFull(sb, enm, innerIndent);
        }

        foreach (var rec in ns.Records)
        {
            AppendRecordFull(sb, rec, innerIndent);
        }

        foreach (var iface in ns.Interfaces)
        {
            AppendInterfaceFull(sb, iface, innerIndent);
        }

        foreach (var strct in ns.Structs)
        {
            AppendStructFull(sb, strct, innerIndent);
        }
    }

    // Append full class definition using the stored definition
    private static void AppendClassFull(StringBuilder sb, ClassInfo cls, string indent)
    {
        // Output the class header
        sb.AppendLine($"{indent}├── class: {cls.Name}");

        // Indent for the class contents
        var innerIndent = indent + "│   ";

        // Append the class definition with proper indentation
        var indentedClass = IndentDefinitionBlock(cls.Definition, innerIndent);
        sb.Append(indentedClass);
    }

    // Append enum full definition
    private static void AppendEnumFull(StringBuilder sb, EnumInfo enm, string indent)
    {
        sb.AppendLine($"{indent}├── enum: {enm.Name}");

        var innerIndent = indent + "│   ";

        var indentedClass = IndentDefinitionBlock(enm.Definition, innerIndent);
        sb.Append(indentedClass);
    }

    // Append record full definition
    private static void AppendRecordFull(StringBuilder sb, RecordInfo rec, string indent)
    {
        sb.AppendLine($"{indent}├── record: {rec.Name}");

        var innerIndent = indent + "│   ";

        var indentedClass = IndentDefinitionBlock(rec.Definition, innerIndent);
        sb.Append(indentedClass);
    }

    // Append interface full definition
    private static void AppendInterfaceFull(StringBuilder sb, InterfaceInfo iface, string indent)
    {
        sb.AppendLine($"{indent}├── interface: {iface.Name}");

        var innerIndent = indent + "│   ";

        var indentedClass = IndentDefinitionBlock(iface.Definition, innerIndent);
        sb.Append(indentedClass);
    }

    // Append struct full definition
    private static void AppendStructFull(StringBuilder sb, StructInfo strct, string indent)
    {
        sb.AppendLine($"{indent}├── struct: {strct.Name}");

        var innerIndent = indent + "│   ";

        var indentedClass = IndentDefinitionBlock(strct.Definition, innerIndent);
        sb.Append(indentedClass);
    }

    private static string IndentDefinitionBlock(string block, string indent)
    {
        // Split the block into lines and apply the indent to each line
        var indentedLines = block.Split('\n').Select(line => indent + line.TrimEnd()); // Trim the end to remove trailing spaces

        // Join the lines back together with newline characters
        return string.Join(Environment.NewLine, indentedLines) + Environment.NewLine;
    }

    private static void AppendClasses(StringBuilder sb, IList<ClassInfo> classes, string indent, bool isFullTree)
    {
        foreach (var classInfo in classes)
        {
            if (isFullTree)
                AppendClassFull(sb, classInfo, indent);
            else
                AppendClassSummary(sb, classInfo, indent);
        }
    }

    private static void AppendEnums(StringBuilder sb, IList<EnumInfo> enums, string indent, bool isFullTree)
    {
        foreach (var enm in enums)
        {
            if (isFullTree)
                AppendEnumFull(sb, enm, indent);
            else
                AppendEnumSummary(sb, enm, indent);
        }
    }

    private static void AppendStructs(StringBuilder sb, IList<StructInfo> structs, string indent, bool isFullTree)
    {
        foreach (var strct in structs)
        {
            if (isFullTree)
                AppendStructFull(sb, strct, indent);
            else
                AppendStructSummary(sb, strct, indent);
        }
    }

    private static void AppendRecords(StringBuilder sb, IList<RecordInfo> records, string indent, bool isFullTree)
    {
        foreach (var rec in records)
        {
            if (isFullTree)
                AppendRecordFull(sb, rec, indent);
            else
                AppendRecordSummary(sb, rec, indent);
        }
    }

    private static void AppendInterfaces(
        StringBuilder sb,
        IList<InterfaceInfo> interfaces,
        string indent,
        bool isFullTree
    )
    {
        foreach (var iface in interfaces)
        {
            if (isFullTree)
                AppendInterfaceFull(sb, iface, indent);
            else
                AppendInterfaceSummary(sb, iface, indent);
        }
    }

    private static void AddFileToRepositoryMap(RepositoryMap repositoryMap, FileInfo fileInfo)
    {
        // Check if the file already exists in the repository map
        var existingFile = repositoryMap.Files.FirstOrDefault(f => f.Path == fileInfo.Path);

        if (existingFile == null)
        {
            // If file doesn't exist, add it to the repository map
            repositoryMap.Files.Add(fileInfo);
        }
        else
        {
            // If file already exists, merge the contents (namespaces, classes, etc.)
            MergeFileInfo(existingFile, fileInfo);
        }
    }

    private static void MergeFileInfo(FileInfo existingFile, FileInfo newFile)
    {
        // Merge namespaces
        foreach (var newNamespace in newFile.Namespaces)
        {
            var existingNamespace = existingFile.Namespaces.FirstOrDefault(n => n.Name == newNamespace.Name);

            if (existingNamespace == null)
            {
                existingFile.Namespaces.Add(newNamespace);
            }
            else
            {
                MergeNamespaceInfo(existingNamespace, newNamespace);
            }
        }

        // Merge top-level classes, enums, structs, etc. that defined outside a specific namespace as top level.
        MergeTypeLists(existingFile.TopLevelStatementsDefinition, newFile.TopLevelStatementsDefinition);

        MergeTypeLists(existingFile.Classes, newFile.Classes);
        MergeTypeLists(existingFile.Enums, newFile.Enums);
        MergeTypeLists(existingFile.Structs, newFile.Structs);
        MergeTypeLists(existingFile.Records, newFile.Records);
        MergeTypeLists(existingFile.Interfaces, newFile.Interfaces);
    }

    private static void MergeNamespaceInfo(NamespaceInfo existingNamespace, NamespaceInfo newNamespace)
    {
        // Merge classes within the namespace
        MergeTypeLists(existingNamespace.Classes, newNamespace.Classes);

        // Merge enums within the namespace
        MergeTypeLists(existingNamespace.Enums, newNamespace.Enums);

        // Merge structs within the namespace
        MergeTypeLists(existingNamespace.Structs, newNamespace.Structs);

        // Merge records within the namespace
        MergeTypeLists(existingNamespace.Records, newNamespace.Records);

        // Merge interfaces within the namespace
        MergeTypeLists(existingNamespace.Interfaces, newNamespace.Interfaces);
    }

    private static void MergeTypeLists<T>(IList<T> existingList, IList<T> newList)
        where T : class
    {
        foreach (var newItem in newList)
        {
            // Check if the item already exists in the list
            if (!existingList.Any(e => e.Equals(newItem)))
            {
                existingList.Add(newItem);
            }
        }
    }
}
