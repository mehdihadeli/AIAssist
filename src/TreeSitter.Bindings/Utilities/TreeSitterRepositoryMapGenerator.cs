using System.Text;
using BuildingBlocks.Extensions;
using TreeSitter.Bindings.CustomTypes.TreeParser;
using static TreeSitter.Bindings.TSBindingsParser;
using static TreeSitter.Bindings.Utilities.TreeSitterParser;
using FileInfo = TreeSitter.Bindings.CustomTypes.TreeParser.FileInfo;
using MethodInfo = TreeSitter.Bindings.CustomTypes.TreeParser.MethodInfo;

namespace TreeSitter.Bindings.Utilities;

public static class TreeSitterRepositoryMapGenerator
{
    /// <summary>
    /// Generate full tree-sitter map based on supported languages. If language is not supported, it will return the code.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GenerateFullTreeSitterRepositoryMap(string code, string path)
    {
        var language = path.GetLanguageFromFilePath();
        if (language is null)
            return code;

        string result;

        unsafe
        {
            var parser = GetParser(language.Value);
            var tree = GetCodeTree(parser, code);
            var defaultQuery = GetLanguageDefaultQuery(language.Value);

            var rootNode = GetRootNode(tree);
            var queryCursor = query_cursor_new();
            query_cursor_exec(queryCursor, defaultQuery, rootNode);

            TSQueryMatch match;

            byte[] byteArrayCode = Encoding.UTF8.GetBytes(code);

            Dictionary<string, List<string>> items = new Dictionary<string, List<string>>();

            while (query_cursor_next_match(queryCursor, &match))
            {
                for (uint i = 0; i < match.capture_count; i++)
                {
                    var capture = match.captures[i];
                    var node = capture.node;

                    string nodeExpression = new string(node_string(node));
                    string nodeType = new GeneratedCString(node_type(node));
                    var grammarSymbol = node_grammar_symbol(node);
                    string grammarType = new GeneratedCString(node_grammar_type(node));

                    var nodeEndStartByte = node_start_byte(node);
                    var nodeEndByte = node_end_byte(node);

                    string codeMatched = Encoding.UTF8.GetString(
                        byteArrayCode,
                        (int)nodeEndStartByte,
                        (int)(nodeEndByte - nodeEndStartByte)
                    );

                    uint length = 0;
                    // Get the capture name, like @name, @definition.class, etc.
                    sbyte* captureNamePtr = query_capture_name_for_id(defaultQuery, capture.index, &length);

                    // Convert the capture name to a string (length is already provided)
                    string captureName = new GeneratedCString(captureNamePtr);

                    if (!items.ContainsKey(captureName))
                    {
                        items.Add(captureName, [codeMatched]);
                    }
                    else
                    {
                        var existsValue = items.GetValueOrDefault(captureName);
                        existsValue?.Add(codeMatched);
                    }
                }
            }

            // Create a formatted result string by joining the items
            result = string.Concat(
                items.Select(kv =>
                {
                    var s = new StringBuilder();
                    kv.Value.ForEach(v => s.Append($"\n{kv.Key}: {v}\n-----------"));

                    return s;
                })
            );
        }

        return result;
    }

    /// <summary>
    /// Generate short tree-sitter map based on supported languages. If language is not supported, it will return the code.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="path"></param>
    /// <param name="repositoryMap"></param>
    /// <returns></returns>
    public static string GenerateSimpleTreeSitterRepositoryMap(string code, string path, RepositoryMap repositoryMap)
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
            MethodInfo? currentMethod = null;

            // Convert code to byte array for extracting matched code using byte positions
            byte[] byteArrayCode = Encoding.UTF8.GetBytes(code);

            // https://tree-sitter.github.io/tree-sitter/using-parsers#walking-trees-with-tree-cursors
            // https://tree-sitter.github.io/tree-sitter/using-parsers#the-query-api
            // https://tree-sitter.github.io/tree-sitter/using-parsers#query-syntax


            while (query_cursor_next_match(queryCursor, &match))
            {
                // Dictionary to group captures by their names
                var captureTags = new Dictionary<string, TSNode>();

                // Populate the dictionary by capture name
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

                    currentClass?.Fields.Add(filedInfo);
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

                    currentClass?.Properties.Add(propertyInfo);
                }

                if (captureTags.ContainsKey(Constants.ClassCaptureTags.Name))
                {
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
                    var currentEnum = new EnumInfo();

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

                if (captureTags.ContainsKey(Constants.StructCaptureTags.Name))
                {
                    var currentStruct = new StructInfo();

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
                    var currentRecord = new RecordInfo();

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
                    var currentInterface = new InterfaceInfo();

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

                if (
                    captureTags.ContainsKey(Constants.MethodCaptureTags.Name)
                    && !captureTags.ContainsKey(Constants.MethodParameterCaptureTags.ParameterName)
                )
                {
                    currentMethod = new MethodInfo();

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

                    currentClass?.Methods.Add(currentMethod);
                }

                // Handling method parameters
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

                    var method = currentClass?.Methods.SingleOrDefault(x => x.Name == methodName);
                    method?.Parameters.Add(parameter);
                }
            }

            AddFileToRepositoryMap(repositoryMap, fileInfo);
        }

        return GenerateTreeString(repositoryMap);
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

        // Merge top-level classes, enums, structs, etc. (if defined outside namespaces)
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

    private static string GenerateTreeString(RepositoryMap repositoryMap)
    {
        var stringBuilder = new StringBuilder();

        foreach (var file in repositoryMap.Files)
        {
            stringBuilder.AppendLine($"{file.Path}:");

            foreach (var ns in file.Namespaces)
            {
                AppendNamespace(stringBuilder, ns, "├── ");
            }

            foreach (var cls in file.Classes)
            {
                AppendClass(stringBuilder, cls, "├── ");
            }
        }

        return stringBuilder.ToString();
    }

    private static void AppendNamespace(StringBuilder sb, NamespaceInfo ns, string prefix)
    {
        sb.AppendLine($"{prefix}namespace {ns.Name}:");

        foreach (var cls in ns.Classes)
        {
            AppendClass(sb, cls, $"{prefix}│   ");
        }

        foreach (var record in ns.Records)
        {
            AppendRecord(sb, record, $"{prefix}│   ");
        }

        foreach (var strct in ns.Structs)
        {
            AppendStruct(sb, strct, $"{prefix}│   ");
        }

        foreach (var enm in ns.Enums)
        {
            AppendEnum(sb, enm, $"{prefix}│   ");
        }

        foreach (var iface in ns.Interfaces)
        {
            AppendInterface(sb, iface, $"{prefix}│   ");
        }
    }

    private static void AppendClass(StringBuilder sb, ClassInfo cls, string prefix)
    {
        sb.AppendLine($"{prefix}class {cls.Name}:");

        // Append fields with their access modifiers
        foreach (var field in cls.Fields)
        {
            sb.AppendLine($"{prefix}│   {field.AccessModifier} {field.Type} {field.Name};");
        }

        // Append properties with their access modifiers
        foreach (var property in cls.Properties)
        {
            sb.AppendLine($"{prefix}│   {property.AccessModifier} {property.Type} {property.Name} {{ get; set; }}");
        }

        // Append methods
        foreach (var method in cls.Methods)
        {
            AppendMethod(sb, method, $"{prefix}│   ");
        }
    }

    private static void AppendMethod(StringBuilder sb, MethodInfo method, string prefix)
    {
        // Get the parameter list as a string
        var parameterList = string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"));

        // Append the access modifier, return type, method name, and parameter list to the StringBuilder
        sb.AppendLine($"{prefix}{method.AccessModifier} {method.ReturnType} {method.Name}({parameterList});");
    }

    private static void AppendStruct(StringBuilder sb, StructInfo strct, string prefix)
    {
        sb.AppendLine($"{prefix}struct {strct.Name}:");
    }

    private static void AppendRecord(StringBuilder sb, RecordInfo record, string prefix)
    {
        sb.AppendLine($"{prefix}record {record.Name}:");
    }

    private static void AppendEnum(StringBuilder sb, EnumInfo enm, string prefix)
    {
        sb.AppendLine($"{prefix}enum {enm.Name}:");
    }

    private static void AppendInterface(StringBuilder sb, InterfaceInfo iface, string prefix)
    {
        sb.AppendLine($"{prefix}interface {iface.Name}:");
    }
}
