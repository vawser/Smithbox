using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;

namespace StudioCore.Help;

[JsonSourceGenerationOptions(WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata, IncludeFields = true)]
[JsonSerializable(typeof(HelpEntry))]
internal partial class HelpEntrySerializerContext : JsonSerializerContext
{
}

public class HelpEntry
{
    public string Author { get; set; }
    public string Title { get; set; }
    public int ProjectType { get; set; }
    public List<string> Tags { get; set; }
    public List<string> Contents { get; set; }
    public List<float> HeaderColor { get; set; }
    public List<float> HighlightColor { get; set; }
}
