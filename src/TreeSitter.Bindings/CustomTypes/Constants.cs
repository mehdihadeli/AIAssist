namespace TreeSitter.Bindings.CustomTypes;

public static class Constants
{
    public static class FileScopedNamespaceCaptureTags
    {
        public const string Name = "name.file_scoped_namespace";
        public const string Definition = "definition.file_scoped_namespace";
    }

    public static class NamespaceCaptureTags
    {
        public const string Name = "name.namespace";
        public const string Definition = "definition.namespace";
    }

    public static class TopLevelStatementCaptureTags
    {
        public const string Definition = "definition.top_level_statement";
    }

    public static class ClassCaptureTags
    {
        public const string Name = "name.class";
        public const string Definition = "definition.class";
        public const string Comment = "comment.class";
    }

    public static class MethodCaptureTags
    {
        public const string Name = "name.method";
        public const string Definition = "definition.method";
        public const string Modifier = "modifier.method";
        public const string Return = "return.method";
        public const string Comment = "comment.method";
    }

    public static class MethodParameterCaptureTags
    {
        public const string MethodName = "method_name.parameter";
        public const string ParameterType = "type.parameter";
        public const string ParameterName = "name.parameter";
    }

    public static class RecordCaptureTags
    {
        public const string Name = "name.record";
        public const string Definition = "definition.record";
        public const string Comment = "comment.record";
    }

    public static class StructCaptureTags
    {
        public const string Name = "name.struct";
        public const string Definition = "definition.struct";
        public const string Comment = "comment.struct";
    }

    public static class InterfaceCaptureTags
    {
        public const string Name = "name.interface";
        public const string Definition = "definition.interface";
        public const string Comment = "comment.interface";
    }

    public static class EnumCaptureTags
    {
        public const string Name = "name.enum";
        public const string Definition = "definition.enum";
        public const string Comment = "comment.enum";
    }

    public static class EnumMemberCaptureTags
    {
        public const string EnumName = "enum_name.member";
        public const string Name = "name.member";
    }

    public const string CommentChildNameTags = "comment.child_name";

    public static class FiledCaptureTags
    {
        public const string Name = "name.field";
        public const string Type = "type.field";
        public const string Definition = "definition.field";
        public const string Modifier = "modifier.field";
    }

    public static class PropertyCaptureTags
    {
        public const string Name = "name.property";
        public const string Type = "type.property";
        public const string Definition = "definition.property";
        public const string Modifier = "modifier.property";
        public const string Comment = "comment.property";
    }
}
