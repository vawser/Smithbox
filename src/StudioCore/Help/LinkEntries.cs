using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StudioCore.Help;

[JsonSourceGenerationOptions(WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata, IncludeFields = true)]
[JsonSerializable(typeof(LinkEntries))]
internal partial class LinkEntriesSerializerContext : JsonSerializerContext
{
}

public class LinkEntries
{
    public List<LinkEntry> Links { get; set; }
}
