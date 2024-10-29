using ColorCode.Common;
using ColorCode.Styling;

namespace BuildingBlocks.SpectreConsole.Themes;

public static class DraculaTheme
{
    private const string DraculaBackground = "#282A36";
    private const string DraculaPlainText = "#F8F8F2";

    private const string DraculaComment = "#6272A4";
    private const string DraculaKeyword = "#FF79C6";
    private const string DraculaString = "#F1FA8C";
    private const string DraculaFunction = "#50FA7B";
    private const string DraculaVariable = "#8BE9FD";
    private const string DraculaOperator = "#FFB86C";
    private const string DraculaClass = "#BD93F9";
    private const string DraculaNumber = "#BD93F9";
    private const string DraculaAttribute = "#FF79C6";
    private const string DraculaXmlDelimiter = "#FF79C6";
    private const string DraculaXmlName = "#8BE9FD";
    private const string DraculaHtmlEntity = "#FFB86C";

    /// <summary>
    /// A Dracula-inspired theme with dark background and vibrant colors.
    /// </summary>
    public static StyleDictionary DraculaDark =>
        [
            new Style(ScopeName.PlainText)
            {
                Foreground = DraculaPlainText,
                Background = DraculaBackground,
                ReferenceName = "plainText",
            },
            new Style(ScopeName.Comment) { Foreground = DraculaComment, ReferenceName = "comment" },
            new Style(ScopeName.Keyword) { Foreground = DraculaKeyword, ReferenceName = "keyword" },
            new Style(ScopeName.String) { Foreground = DraculaString, ReferenceName = "string" },
            new Style(ScopeName.Number) { Foreground = DraculaNumber, ReferenceName = "number" },
            new Style(ScopeName.BuiltinFunction) { Foreground = DraculaFunction, ReferenceName = "function" },
            new Style(ScopeName.TypeVariable) { Foreground = DraculaVariable, ReferenceName = "variable" },
            new Style(ScopeName.ClassName) { Foreground = DraculaClass, ReferenceName = "className" },
            new Style(ScopeName.Attribute) { Foreground = DraculaAttribute, ReferenceName = "attribute" },
            new Style(ScopeName.Operator) { Foreground = DraculaOperator, ReferenceName = "operator" },
            new Style(ScopeName.XmlDelimiter) { Foreground = DraculaXmlDelimiter, ReferenceName = "xmlDelimiter" },
            new Style(ScopeName.XmlName) { Foreground = DraculaXmlName, ReferenceName = "xmlName" },
            new Style(ScopeName.HtmlEntity) { Foreground = DraculaHtmlEntity, ReferenceName = "htmlEntity" },
            new Style(ScopeName.XmlAttributeValue) { Foreground = DraculaString, ReferenceName = "xmlAttributeValue" },
            new Style(ScopeName.XmlComment) { Foreground = DraculaComment, ReferenceName = "xmlComment" },
            new Style(ScopeName.Delimiter) { Foreground = DraculaPlainText, ReferenceName = "delimiter" },
        ];
}
