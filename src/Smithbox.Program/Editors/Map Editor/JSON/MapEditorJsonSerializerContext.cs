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

// Map Object Names
[JsonSerializable(typeof(MapObjectNameMapEntry))]
[JsonSerializable(typeof(MapObjectNameEntry))]

// Asset Masks (ER)
[JsonSerializable(typeof(AssetMasks))]
[JsonSerializable(typeof(AssetMaskEntry))]
[JsonSerializable(typeof(AssetMaskSection))]

// Spawn States (DS2)
[JsonSerializable(typeof(SpawnStates))]
[JsonSerializable(typeof(SpawnStateEntry))]
[JsonSerializable(typeof(SpawnStateContents))]

internal partial class MapEditorJsonSerializerContext : JsonSerializerContext
{
}

