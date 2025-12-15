using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.MapEditor;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

[JsonSerializable(typeof(MapListFilterCollection))]
[JsonSerializable(typeof(MapListFilterSet))]

internal partial class MapEditorJsonSerializerContext : JsonSerializerContext
{
}

public class MapListFilterCollection
{
    public List<MapListFilterSet> Entries { get; set; }
}

public class MapListFilterSet
{
    public Guid ID { get; set; }
    public FilterType Type { get; set; }
    public string Name { get; set; }
    public List<string> Entries { get; set; }
}
