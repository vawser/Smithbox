using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StudioCore.Program.Editors.TextEditor;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

[JsonSerializable(typeof(TextExportList))]
[JsonSerializable(typeof(TextExportEntry))]

internal partial class TextJsonSerializerContext : JsonSerializerContext
{
}

public class TextExportList
{
    public List<TextExportEntry> Entries { get; set; }
}

public class TextExportEntry
{
    public string ContainerName { get; set; }
    public string FmgName { get; set; }
    public int EntryID { get; set; }
    public string EntryText { get; set; }
}
