using System.Text;
using BuildingBlocks.Extensions;
using TreeSitter.Bindings.CustomTypes.TreeParser;
using static TreeSitter.Bindings.TSBindingsParser;
using static TreeSitter.Bindings.Utilities.TreeSitterParser;
using FileInfo = TreeSitter.Bindings.CustomTypes.TreeParser.FileInfo;
using MethodInfo = TreeSitter.Bindings.CustomTypes.TreeParser.MethodInfo;

namespace TreeSitter.Bindings.Utilities;

// ref: https://tree-sitter.github.io/tree-sitter/using-parsers

public static class TreeSitterRepositoryMapGenerator
{
    /// <summary>
    /// Generate short tree-sitter map based on supported languages. If language is not supported, it will return the code.
    /// </summary>
    /// <param Name="code"></param>
    /// <param Name="path"></param>
    /// <param Name="repositoryMap"></param>
    /// <param Name="writeFullTree"></param>
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
            NamespaceInfo? currentNamespace = null;
            ClassInfo? currentClass = null;
            RecordInfo? currentRecord = null;
            StructInfo? currentStruct = null;
            InterfaceInfo? currentInterface = null;
            EnumInfo? currentEnum = null;

            // Convert code to byte array for extracting matched code using byte positions
            byte[] byteArrayCode = Encoding.UTF8.GetBytes(code);

            // https://tree-sitter.github.io/tree-sitter/using-parsers#walking-trees-with-tree-cursors
            // https://tree-sitter.github.io/tree-sitter/using-parsers#the-query-api
            // https://tree-sitter.github.io/tree-sitter/using-parsers#query-syntax


            while (query_cursor_next_match(queryCursor, &match))
            {
                // Dictionary to group captures by their names
                var captureTags = new Dictionary<string, TSNode>();

                // Populate the dictionary by capture Name
                for (int i = 0; i < match.capture_count; i++)
                {
                    var capture = match.captures[i];
                    var node = capture.node;

                    uint length = 0;

                    sbyte* captureNamePtr = query_capture_name_for_id(defaultQuery, capture.index, &length);

                    string captureName = new GeneratedCString(captureNamePtr);

                    captureTags.Add(captureName, node);
                }

                if (
                    captureTags.ContainsKey(Constants.NamespaceCaptureTags.Name)
                    || captureTags.ContainsKey(Constants.FileScopedNamespaceCaptureTags.Name)
                )
                {
                    currentNamespace = new NamespaceInfo();

                    foreach (var captureTag in captureTags)
                    {
                        var namespaceNode = captureTag.Value;
                        var namespaceCaptureValue = GetMatchedCode(byteArrayCode, namespaceNode);

                        switch (captureTag.Key)
                        {
                            case Constants.NamespaceCaptureTags.Name:
                            case Constants.FileScopedNamespaceCaptureTags.Name:
                                currentNamespace.Name = namespaceCaptureValue;
                                break;
                            case Constants.NamespaceCaptureTags.Definition:
                            case Constants.FileScopedNamespaceCaptureTags.Definition:
                                currentNamespace.Definition = namespaceCaptureValue;
                                break;
                        }
                    }

                    fileInfo.Namespaces.Add(currentNamespace);
                }

                if (captureTags.ContainsKey(Constants.TopLevelStatementCaptureTags.Definition))
                {
                    foreach (var captureTag in captureTags)
                    {
                        var topLevelStatementNode = captureTag.Value;

                        var topLevelStatementCaptureValue = GetMatchedCode(byteArrayCode, topLevelStatementNode);

                        switch (captureTag.Key)
                        {
                            case Constants.TopLevelStatementCaptureTags.Definition:
                                fileInfo.TopLevelStatementsDefinition.Add(topLevelStatementCaptureValue);

                                break;
                        }
                    }
                }

                if (captureTags.ContainsKey(Constants.FiledCaptureTags.Name))
                {
                    var filedInfo = new FieldInfo();

                    foreach (var captureTag in captureTags)
                    {
                        var fieldNode = captureTag.Value;
                        var fieldCaptureValue = GetMatchedCode(byteArrayCode, fieldNode);

                        switch (captureTag.Key)
                        {
                            case Constants.FiledCaptureTags.Name:
                                filedInfo.Name = fieldCaptureValue;
                                break;
                            case Constants.FiledCaptureTags.Type:
                                filedInfo.Type = fieldCaptureValue;
                                break;
                            case Constants.FiledCaptureTags.Modifier:
                                filedInfo.AccessModifier = fieldCaptureValue;
                                break;
                            case Constants.FiledCaptureTags.Definition:
                                filedInfo.Definition = fieldCaptureValue;
                                break;
                        }
                    }

                    if (currentClass is not null)
                    {
                        currentClass.Fields.Add(filedInfo);
                    }

                    if (currentRecord is not null)
                    {
                        currentRecord.Fields.Add(filedInfo);
                    }

                    if (currentStruct is not null)
                    {
                        currentStruct.Fields.Add(filedInfo);
                    }
                }

                if (captureTags.ContainsKey(Constants.PropertyCaptureTags.Name))
                {
                    var propertyInfo = new PropertyInfo();

                    foreach (var captureTag in captureTags)
                    {
                        var propertyNode = captureTag.Value;
                        var propertyCaptureValue = GetMatchedCode(byteArrayCode, propertyNode);

                        switch (captureTag.Key)
                        {
                            case Constants.PropertyCaptureTags.Name:
                                propertyInfo.Name = propertyCaptureValue;
                                break;
                            case Constants.PropertyCaptureTags.Type:
                                propertyInfo.Type = propertyCaptureValue;
                                break;
                            case Constants.PropertyCaptureTags.Modifier:
                                propertyInfo.AccessModifier = propertyCaptureValue;
                                break;
                            case Constants.PropertyCaptureTags.Definition:
                                propertyInfo.Definition = propertyCaptureValue;
                                break;
                        }
                    }

                    if (currentClass is not null)
                    {
                        currentClass.Properties.Add(propertyInfo);
                    }

                    if (currentRecord is not null)
                    {
                        currentRecord.Properties.Add(propertyInfo);
                    }

                    if (currentStruct is not null)
                    {
                        currentStruct.Properties.Add(propertyInfo);
                    }

                    if (currentInterface is not null)
                    {
                        currentInterface.Properties.Add(propertyInfo);
                    }
                }

                if (captureTags.ContainsKey(Constants.ClassCaptureTags.Name))
                {
                    // resting `currentRecord` here for the grammar query
                    currentRecord = null;
                    // resting `currentStruct` here for the grammar query
                    currentStruct = null;
                    // resting currentInterface here for the grammar query
                    currentInterface = null;
                    // resting `currentClass` here for the grammar query
                    currentClass = new ClassInfo();

                    foreach (var captureTag in captureTags)
                    {
                        var classNode = captureTag.Value;
                        var classCaptureValue = GetMatchedCode(byteArrayCode, classNode);

                        switch (captureTag.Key)
                        {
                            case Constants.ClassCaptureTags.Name:
                                currentClass.Name = classCaptureValue;
                                break;
                            case Constants.ClassCaptureTags.Definition:
                                currentClass.Definition = classCaptureValue;
                                break;
                        }
                    }

                    if (currentNamespace != null)
                    {
                        currentNamespace.Classes.Add(currentClass);
                    }
                    else
                    {
                        fileInfo.Classes.Add(currentClass);
                    }
                }

                if (captureTags.ContainsKey(Constants.EnumCaptureTags.Name))
                {
                    // resting `currentClass` here for the grammar query
                    currentClass = null;
                    // resting `currentRecord` here for the grammar query
                    currentRecord = null;
                    // resting `currentStruct` here for the grammar query
                    currentStruct = null;
                    // resting currentInterface here for the grammar query
                    currentInterface = null;
                    // resting currentEnum here for the grammar query
                    currentEnum = new EnumInfo();

                    foreach (var captureTag in captureTags)
                    {
                        var enumNode = captureTag.Value;
                        var enumCaptureValue = GetMatchedCode(byteArrayCode, enumNode);

                        switch (captureTag.Key)
                        {
                            case Constants.EnumCaptureTags.Name:
                                currentEnum.Name = enumCaptureValue;
                                break;
                            case Constants.EnumCaptureTags.Definition:
                                currentEnum.Definition = enumCaptureValue;
                                break;
                        }
                    }

                    if (currentNamespace != null)
                    {
                        currentNamespace.Enums.Add(currentEnum);
                    }
                    else
                    {
                        fileInfo.Enums.Add(currentEnum);
                    }
                }

                if (
                    captureTags.ContainsKey(Constants.EnumMemberCaptureTags.Name)
                    && captureTags.ContainsKey(Constants.EnumMemberCaptureTags.EnumName)
                )
                {
                    string enumMemberName = string.Empty;
                    string enumName = string.Empty;

                    foreach (var captureTag in captureTags)
                    {
                        var enumMemberNode = captureTag.Value;
                        var enumMemberCaptureValue = GetMatchedCode(byteArrayCode, enumMemberNode);

                        switch (captureTag.Key)
                        {
                            case Constants.EnumMemberCaptureTags.Name:
                                enumMemberName = enumMemberCaptureValue;
                                break;
                            case Constants.EnumMemberCaptureTags.EnumName:
                                enumName = enumMemberCaptureValue;
                                break;
                        }
                    }

                    if (currentNamespace != null)
                    {
                        var enumInfo = currentNamespace.Enums.SingleOrDefault(x => x.Name == enumName);

                        enumInfo?.Members.Add(enumMemberName);
                    }
                    else
                    {
                        var enumInfo = fileInfo.Enums.SingleOrDefault(x => x.Name == enumName);
                        enumInfo?.Members.Add(enumMemberName);
                    }
                }

                if (captureTags.ContainsKey(Constants.StructCaptureTags.Name))
                {
                    // resting `currentClass` here for the grammar query
                    currentClass = null;
                    // resting `currentRecord` here for the grammar query
                    currentRecord = null;
                    // resting currentInterface here for the grammar query
                    currentInterface = null;
                    // resting `currentStruct` here for the grammar query
                    currentStruct = new StructInfo();

                    foreach (var captureTag in captureTags)
                    {
                        var structNode = captureTag.Value;
                        var structCaptureValue = GetMatchedCode(byteArrayCode, structNode);

                        switch (captureTag.Key)
                        {
                            case Constants.StructCaptureTags.Name:
                                currentStruct.Name = structCaptureValue;
                                break;
                            case Constants.StructCaptureTags.Definition:
                                currentStruct.Definition = structCaptureValue;
                                break;
                        }
                    }

                    if (currentNamespace != null)
                    {
                        currentNamespace.Structs.Add(currentStruct);
                    }
                    else
                    {
                        fileInfo.Structs.Add(currentStruct);
                    }
                }

                if (captureTags.ContainsKey(Constants.RecordCaptureTags.Name))
                {
                    // resting `currentClass` here for the grammar query
                    currentClass = null;
                    // resting `currentStruct` here for the grammar query
                    currentStruct = null;
                    // resting currentInterface here for the grammar query
                    currentInterface = null;
                    // resting `currentRecord` here for the grammar query
                    currentRecord = new RecordInfo();

                    foreach (var captureTag in captureTags)
                    {
                        var recordNode = captureTag.Value;
                        var recordCaptureValue = GetMatchedCode(byteArrayCode, recordNode);

                        switch (captureTag.Key)
                        {
                            case Constants.RecordCaptureTags.Name:
                                currentRecord.Name = recordCaptureValue;
                                break;
                            case Constants.RecordCaptureTags.Definition:
                                currentRecord.Definition = recordCaptureValue;
                                break;
                        }
                    }

                    if (currentNamespace != null)
                    {
                        currentNamespace.Records.Add(currentRecord);
                    }
                    else
                    {
                        fileInfo.Records.Add(currentRecord);
                    }
                }

                if (captureTags.ContainsKey(Constants.InterfaceCaptureTags.Name))
                {
                    // resting `currentClass` here for the grammar query
                    currentClass = null;
                    // resting `currentRecord` here for the grammar query
                    currentRecord = null;
                    // resting `currentStruct` here for the grammar query
                    currentStruct = null;
                    // resting currentInterface here for the grammar query
                    currentInterface = new InterfaceInfo();

                    foreach (var captureTag in captureTags)
                    {
                        var interfaceNode = captureTag.Value;
                        var interfaceCaptureValue = GetMatchedCode(byteArrayCode, interfaceNode);

                        switch (captureTag.Key)
                        {
                            case Constants.InterfaceCaptureTags.Name:
                                currentInterface.Name = interfaceCaptureValue;
                                break;
                            case Constants.InterfaceCaptureTags.Definition:
                                currentInterface.Definition = interfaceCaptureValue;
                                break;
                        }
                    }

                    if (currentNamespace != null)
                    {
                        currentNamespace.Interfaces.Add(currentInterface);
                    }
                    else
                    {
                        fileInfo.Interfaces.Add(currentInterface);
                    }
                }

                if (captureTags.ContainsKey(Constants.MethodCaptureTags.Name))
                {
                    var currentMethod = new MethodInfo();

                    foreach (var captureTag in captureTags)
                    {
                        var methodNode = captureTag.Value;
                        var methodCaptureValue = GetMatchedCode(byteArrayCode, methodNode);

                        switch (captureTag.Key)
                        {
                            case Constants.MethodCaptureTags.Name:
                                currentMethod.Name = methodCaptureValue;
                                break;
                            case Constants.MethodCaptureTags.Return:
                                currentMethod.ReturnType = methodCaptureValue;
                                break;
                            case Constants.MethodCaptureTags.Modifier:
                                currentMethod.AccessModifier = methodCaptureValue;
                                break;
                            case Constants.MethodCaptureTags.Definition:
                                currentMethod.Definition = methodCaptureValue;
                                break;
                        }
                    }

                    if (currentClass is not null)
                    {
                        currentClass.Methods.Add(currentMethod);
                    }

                    if (currentRecord is not null)
                    {
                        currentRecord.Methods.Add(currentMethod);
                    }

                    if (currentStruct is not null)
                    {
                        currentStruct.Methods.Add(currentMethod);
                    }

                    if (currentInterface is not null)
                    {
                        currentInterface.Methods.Add(currentMethod);
                    }
                }

                if (
                    captureTags.ContainsKey(Constants.MethodParameterCaptureTags.ParameterType)
                    && captureTags.ContainsKey(Constants.MethodParameterCaptureTags.ParameterName)
                )
                {
                    var parameter = new ParameterInfo();
                    string methodName = string.Empty;

                    foreach (var captureTag in captureTags)
                    {
                        var parameterNode = captureTag.Value;
                        var parameterCaptureValue = GetMatchedCode(byteArrayCode, parameterNode);

                        switch (captureTag.Key)
                        {
                            case Constants.MethodParameterCaptureTags.MethodName:
                                methodName = parameterCaptureValue;
                                break;
                            case Constants.MethodParameterCaptureTags.ParameterName:
                                parameter.Name = parameterCaptureValue;
                                break;
                            case Constants.MethodParameterCaptureTags.ParameterType:
                                parameter.Type = parameterCaptureValue;
                                break;
                        }
                    }

                    if (currentClass is not null)
                    {
                        var method = currentClass.Methods.SingleOrDefault(x => x.Name == methodName);

                        method?.Parameters.Add(parameter);
                    }

                    if (currentRecord is not null)
                    {
                        var method = currentRecord.Methods.SingleOrDefault(x => x.Name == methodName);

                        method?.Parameters.Add(parameter);
                    }

                    if (currentStruct is not null)
                    {
                        var method = currentStruct.Methods.SingleOrDefault(x => x.Name == methodName);

                        method?.Parameters.Add(parameter);
                    }

                    if (currentInterface is not null)
                    {
                        var method = currentInterface.Methods.SingleOrDefault(x => x.Name == methodName);

                        method?.Parameters.Add(parameter);
                    }
                }
            }

            AddFileToRepositoryMap(repositoryMap, fileInfo);
        }

        return GenerateTreeString(repositoryMap, applicationName: "root", writeFullTree: writeFullTree);
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
