using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.MapEditor;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

// Map List Filter
[JsonSerializable(typeof(MapListFilterCollection))]
[JsonSerializable(typeof(MapListFilterSet))]

// Entity Selection Group
[JsonSerializable(typeof(EntitySelectionGroupList))]
[JsonSerializable(typeof(EntitySelectionGroupResource))]

// Spawn States
[JsonSerializable(typeof(SpawnStateResource))]
[JsonSerializable(typeof(SpawnStateEntry))]
[JsonSerializable(typeof(SpawnStatePair))]

// Map Object Names
[JsonSerializable(typeof(MapObjectNameMapEntry))]
[JsonSerializable(typeof(MapObjectNameEntry))]

internal partial class MapEditorJsonSerializerContext : JsonSerializerContext
{
}

