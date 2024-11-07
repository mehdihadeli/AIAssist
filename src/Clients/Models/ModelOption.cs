using System.Text.Json.Serialization;
using Clients.Converters;

namespace Clients.Models;

public class ModelOption
{
    [JsonConverter(typeof(CodeDiffTypeConverter))]
    public CodeDiffType CodeDiffType { get; set; }

    [JsonConverter(typeof(CodeAssistTypeConverter))]
    public CodeAssistType CodeAssistType { get; set; }
    public decimal Threshold { get; set; }
    public decimal Temperature { get; set; }
}
