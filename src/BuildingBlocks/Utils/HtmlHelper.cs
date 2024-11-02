using System.Globalization;
using System.Text;
using AngleSharp;
using AngleSharp.Dom;

namespace BuildingBlocks.Utils;

public static class HtmlHelper
{
    // ref: https://www.bbcode.org/reference.php
    public static string ConvertHtmlToBBCode(string html)
    {
        // Configure AngleSharp's context to parse the HTML document
        var config = Configuration.Default;
        var context = BrowsingContext.New(config);
        var document = context.OpenAsync(req => req.Content(html)).Result;

        var bbCodeBuilder = new StringBuilder();
        ParseHtmlNode(document.Body, bbCodeBuilder);

        return bbCodeBuilder.ToString();
    }

    static void ParseHtmlNode(INode node, StringBuilder bbCodeBuilder)
    {
        if (node is IText textNode)
        {
            // Append text directly
            bbCodeBuilder.Append(textNode.TextContent);
        }
        else if (node is IElement element)
        {
            if (element.TagName.Equals("H1", StringComparison.OrdinalIgnoreCase))
            {
                bbCodeBuilder.Append($"[b][u]{element.TextContent}[/][/]\n");
            }
            else if (element.TagName.Equals("H2", StringComparison.OrdinalIgnoreCase))
            {
                bbCodeBuilder.Append($"[b]{element.TextContent}[/]\n");
            }
            else if (element.TagName.Equals("P", StringComparison.OrdinalIgnoreCase))
            {
                bbCodeBuilder.Append($"\n{element.TextContent}\n");
            }
            else if (element.TagName.Equals("PRE", StringComparison.OrdinalIgnoreCase))
            {
                bbCodeBuilder.Append("\n");
                foreach (var child in element.ChildNodes)
                    ParseHtmlNode(child, bbCodeBuilder);
            }
            else if (
                element.TagName.Equals("SPAN", StringComparison.OrdinalIgnoreCase) && element.HasAttribute("style")
            )
            {
                ApplyInlineStyles(element, bbCodeBuilder);
            }
            else
            {
                // Process children for other tags or nested elements
                foreach (var child in element.ChildNodes)
                    ParseHtmlNode(child, bbCodeBuilder);
            }
        }
    }

    private static void ApplyInlineStyles(IElement element, StringBuilder bbCodeBuilder)
    {
        // Start with a string builder for the formatted content
        var formattedText = new StringBuilder();

        if (element.HasAttribute("style"))
        {
            var style = element.GetAttribute("style");
            var styleParts = style.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in styleParts)
            {
                var propertyValue = part.Split(':');
                if (propertyValue.Length != 2)
                    continue;

                var property = propertyValue[0].Trim().ToLower(CultureInfo.InvariantCulture);
                var value = propertyValue[1].Trim();

                switch (property)
                {
                    case "color":
                        var hexColor = value;
                        formattedText.Append($"[{hexColor}]");
                        break;

                    case "font-weight":
                        if (value.Equals("bold", StringComparison.OrdinalIgnoreCase))
                        {
                            formattedText.Append("[b]");
                        }

                        break;

                    case "font-style":
                        if (value.Equals("italic", StringComparison.OrdinalIgnoreCase))
                        {
                            formattedText.Append("[i]");
                        }

                        break;

                    case "text-decoration":
                        if (value.Equals("underline", StringComparison.OrdinalIgnoreCase))
                        {
                            formattedText.Append("[u]");
                        }
                        break;
                }
            }
        }

        // Append the text content with formatting
        formattedText.Append(element.TextContent);

        // Close the tags in reverse order to match opening
        if (element.HasAttribute("style"))
        {
            var style = element.GetAttribute("style");
            var styleParts = style.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in styleParts)
            {
                var propertyValue = part.Split(':');
                if (propertyValue.Length != 2)
                    continue;

                var property = propertyValue[0].Trim().ToLower(CultureInfo.InvariantCulture);
                var value = propertyValue[1].Trim();

                switch (property)
                {
                    case "color":
                        formattedText.Append("[/]");
                        break;

                    case "font-weight":
                        if (value.Equals("bold", StringComparison.OrdinalIgnoreCase))
                        {
                            formattedText.Append("[/b]");
                        }

                        break;

                    case "font-style":
                        if (value.Equals("italic", StringComparison.OrdinalIgnoreCase))
                        {
                            formattedText.Append("[/i]");
                        }

                        break;

                    case "text-decoration":
                        if (value.Equals("underline", StringComparison.OrdinalIgnoreCase))
                        {
                            formattedText.Append("[/u]");
                        }

                        break;

                    // Add more styles as necessary
                }
            }
        }

        bbCodeBuilder.Append(formattedText);
    }
}
