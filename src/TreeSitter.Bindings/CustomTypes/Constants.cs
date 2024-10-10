namespace TreeSitter.Bindings;

public static class Constants
{
    public static class FileScopedNamespaceCaptureTags
    {
        public const string Name = "name.file_scope_namespace";
        public const string Definition = "definition.file_scope_namespace";
    }

    public static class NamespaceCaptureTags
    {
        public const string Name = "name.namespace";
        public const string Definition = "definition.namespace";
    }

    public static class ClassCaptureTags
    {
        public const string Name = "name.class";
        public const string Definition = "definition.class";
    }

    public static class MethodCaptureTags
    {
        public const string Name = "name.method";
        public const string Definition = "definition.method";
        public const string Modifier = "modifier.method";
        public const string Return = "return.method";
    }

    public static class MethodParameterCaptureTags
    {
        public const string MethodName = "name.method";
        public const string ParameterType = "parameter.type";
        public const string ParameterName = "parameter.name";
    }

    public static class RecordCaptureTags
    {
        public const string Name = "name.record";
        public const string Definition = "definition.record";
    }

    public static class StructCaptureTags
    {
        public const string Name = "name.struct";
        public const string Definition = "definition.struct";
    }

    public static class InterfaceCaptureTags
    {
        public const string Name = "name.interface";
        public const string Definition = "definition.interface";
    }

    public static class EnumCaptureTags
    {
        public const string Name = "name.enum";
        public const string Definition = "definition.enum";
    }

    public static class FiledCaptureTags
    {
        public const string Name = "name.filed";
        public const string Type = "type.filed";
        public const string Definition = "definition.filed";
        public const string Modifier = "modifier.filed";
    }

    public static class PropertyCaptureTags
    {
        public const string Name = "name.property";
        public const string Type = "type.property";
        public const string Definition = "definition.property";
        public const string Modifier = "modifier.property";
    }
}
