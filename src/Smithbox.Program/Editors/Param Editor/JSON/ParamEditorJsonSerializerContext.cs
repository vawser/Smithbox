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

// Param Annotations
[JsonSerializable(typeof(ParamAnnotations))]
[JsonSerializable(typeof(ParamAnnotationEntry))]
[JsonSerializable(typeof(ParamAnnotationFieldEntry))]

[JsonSerializable(typeof(ParamAnnotationLanguages))]
[JsonSerializable(typeof(ParamAnnotationLanguageEntry))]

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
[JsonSerializable(typeof(ParamReloaderOffsets))]
[JsonSerializable(typeof(ParamReloaderOffsetEntry))]

// Param Categories
[JsonSerializable(typeof(ParamCategories))]
[JsonSerializable(typeof(ParamCategoryEntry))]
[JsonSerializable(typeof(ParamCategoryNameEntry))]

// Commutative Param Groups
[JsonSerializable(typeof(ParamCommutativityGroups))]
[JsonSerializable(typeof(ParamCommutativityEntry))]

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
[JsonSerializable(typeof(GraphAnnotations))]
[JsonSerializable(typeof(GraphAnnotationEntry))]

// Delta Patch
[JsonSerializable(typeof(ParamDeltaPatch))]
[JsonSerializable(typeof(ParamDelta))]
[JsonSerializable(typeof(RowDelta))]
[JsonSerializable(typeof(FieldDelta))]

// Group References
[JsonSerializable(typeof(FieldReferenceGroups))]
[JsonSerializable(typeof(FieldReferenceGroup))]
[JsonSerializable(typeof(FieldReferenceSet))]

// Field Groups
[JsonSerializable(typeof(FieldLayouts))]
[JsonSerializable(typeof(FieldLayout))]
[JsonSerializable(typeof(FieldLayoutEntry))]

// Param Enums
[JsonSerializable(typeof(ParamEnums))]
[JsonSerializable(typeof(ParamEnumEntry))]
[JsonSerializable(typeof(ParamEnumOption))]
[JsonSerializable(typeof(ParamCategoryTextEntry))]


internal partial class ParamEditorJsonSerializerContext : JsonSerializerContext
{
}

