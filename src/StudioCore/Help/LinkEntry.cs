namespace StudioCore.Help;
using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata, IncludeFields = true)]
[JsonSerializable(typeof(LinkEntry))]
internal partial class LinkEntrySerializerContext : JsonSerializerContext
{
}

public class LinkEntry
{
    public string Title { get; set; }
    public string URL { get; set; }
}
