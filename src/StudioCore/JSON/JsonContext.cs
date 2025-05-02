using StudioCore.Banks.FormatBank;
using StudioCore.Core;
using StudioCore.Core.ProjectNS;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.Resources.JSON;

// Common serializer context for JSON generation
[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

[JsonSerializable(typeof(CFG))]
[JsonSerializable(typeof(UI))]
[JsonSerializable(typeof(KeyBindings.Bindings))]
[JsonSerializable(typeof(KeyBind))]

// Project
[JsonSerializable(typeof(Project))]
[JsonSerializable(typeof(ProjectDisplay))]

[JsonSerializable(typeof(AliasStore))]
[JsonSerializable(typeof(AliasEntry))]

[JsonSerializable(typeof(FileDictionary))]
[JsonSerializable(typeof(FileDictionaryEntry))]

// Map Editor
[JsonSerializable(typeof(MassEditTemplate))]

[JsonSerializable(typeof(EntitySelectionGroupList))]
[JsonSerializable(typeof(EntitySelectionEntry))]

[JsonSerializable(typeof(SpawnStateResource))]
[JsonSerializable(typeof(SpawnStateEntry))]
[JsonSerializable(typeof(SpawnStatePair))]

[JsonSerializable(typeof(FormatMask))]
[JsonSerializable(typeof(FormatMaskEntry))]
[JsonSerializable(typeof(MaskSection))]

// 

internal partial class SmithboxSerializerContext : JsonSerializerContext
{
}

