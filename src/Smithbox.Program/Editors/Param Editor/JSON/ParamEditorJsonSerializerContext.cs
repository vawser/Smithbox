using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]


// Icon Configurations
[JsonSerializable(typeof(IconConfigurations))]
[JsonSerializable(typeof(IconConfigurationEntry))]

// Table Group Names
[JsonSerializable(typeof(TableGroupNameStore))]
[JsonSerializable(typeof(TableGroupParamEntry))]
[JsonSerializable(typeof(TableGroupEntry))]

// Table Params
[JsonSerializable(typeof(TableParams))]

// Param Memory Offsets
[JsonSerializable(typeof(GameOffsetResource))]
[JsonSerializable(typeof(GameOffsetReference))]

// Param Categories
[JsonSerializable(typeof(ParamCategoryResource))]
[JsonSerializable(typeof(ParamCategoryEntry))]

// Commutative Param Groups
[JsonSerializable(typeof(ParamCommutativeResource))]
[JsonSerializable(typeof(ParamCommutativeEntry))]

// Param Type Info
[JsonSerializable(typeof(ParamTypeInfo))]

// Row Names
[JsonSerializable(typeof(RowNameStore))]
[JsonSerializable(typeof(RowNameStoreLegacy))]
[JsonSerializable(typeof(RowNameParam))]
[JsonSerializable(typeof(RowNameParamLegacy))]
[JsonSerializable(typeof(RowNameEntry))]
[JsonSerializable(typeof(RowNameEntryLegacy))]

// Param Upgrader Instructions
[JsonSerializable(typeof(ParamUpgraderInfo))]
[JsonSerializable(typeof(OldRegulationEntry))]
[JsonSerializable(typeof(UpgraderMassEditEntry))]

// Graph Legends
[JsonSerializable(typeof(GraphLegends))]
[JsonSerializable(typeof(GraphLegendEntry))]

internal partial class ParamEditorJsonSerializerContext : JsonSerializerContext
{
}

