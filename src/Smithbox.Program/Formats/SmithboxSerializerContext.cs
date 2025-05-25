using Octokit;
using StudioCore.Configuration;
using StudioCore.Core;
using System.Text.Json.Serialization;

namespace StudioCore.Formats.JSON;

// Common serializer context for JSON generation
[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

// Program
[JsonSerializable(typeof(CFG))]
[JsonSerializable(typeof(UI))]
[JsonSerializable(typeof(KeyBindings.Bindings))]
[JsonSerializable(typeof(KeyBind))]

// Project
[JsonSerializable(typeof(ProjectEntry))]
[JsonSerializable(typeof(ProjectDisplayOrder))]

[JsonSerializable(typeof(FileDictionary))]
[JsonSerializable(typeof(FileDictionaryEntry))]

// Alias Data
[JsonSerializable(typeof(AliasStore))]
[JsonSerializable(typeof(AliasEntry))]

// Project Enums
[JsonSerializable(typeof(ProjectEnumResource))]
[JsonSerializable(typeof(ProjectEnumEntry))]
[JsonSerializable(typeof(ProjectEnumOption))]

// Format Information
[JsonSerializable(typeof(FormatResource))]
[JsonSerializable(typeof(FormatReference))]
[JsonSerializable(typeof(FormatMember))]

[JsonSerializable(typeof(FormatEnum))]
[JsonSerializable(typeof(FormatEnumEntry))]
[JsonSerializable(typeof(FormatEnumMember))]

[JsonSerializable(typeof(FormatMask))]
[JsonSerializable(typeof(FormatMaskEntry))]
[JsonSerializable(typeof(MaskSection))]

// Entity Selection Group
[JsonSerializable(typeof(EntitySelectionGroupList))]
[JsonSerializable(typeof(EntitySelectionGroupResource))]

// Param Memory Offsets
[JsonSerializable(typeof(GameOffsetResource))]
[JsonSerializable(typeof(GameOffsetReference))]

// Param Categories
[JsonSerializable(typeof(ParamCategoryResource))]
[JsonSerializable(typeof(ParamCategoryEntry))]

// Commutative Param Groups
[JsonSerializable(typeof(ParamCommutativeResource))]
[JsonSerializable(typeof(ParamCommutativeEntry))]

// Spawn States
[JsonSerializable(typeof(SpawnStateResource))]
[JsonSerializable(typeof(SpawnStateEntry))]
[JsonSerializable(typeof(SpawnStatePair))]

// Param Type Info
[JsonSerializable(typeof(ParamTypeInfo))]

// Row Names
[JsonSerializable(typeof(RowNameStore))]
[JsonSerializable(typeof(RowNameParam))]
[JsonSerializable(typeof(RowNameEntry))]

// Param Upgrader Instructions
[JsonSerializable(typeof(ParamUpgraderInfo))]
[JsonSerializable(typeof(OldRegulationEntry))]
[JsonSerializable(typeof(UpgraderMassEditEntry))]

// Graph Legends
[JsonSerializable(typeof(GraphLegends))]
[JsonSerializable(typeof(GraphLegendEntry))]

// Material Display Configuration
[JsonSerializable(typeof(MaterialDisplayConfiguration))]
[JsonSerializable(typeof(MaterialFileListConfiguration))]

internal partial class SmithboxSerializerContext : JsonSerializerContext
{
}
