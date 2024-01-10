using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StudioCore.Help;

[JsonSourceGenerationOptions(WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata, IncludeFields = true)]
[JsonSerializable(typeof(TextEntry))]
internal partial class TextEntrySerializerContext : JsonSerializerContext
{
}

public class TextEntry
{
    public List<string> Text { get; set; }
}
