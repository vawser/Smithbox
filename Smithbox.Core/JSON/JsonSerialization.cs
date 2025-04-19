using StudioCore.Banks.GameOffsetBank;
using StudioCore.Banks.ParamCategoryBank;
using StudioCore.Banks.ProjectEnumBank;
using StudioCore.Interface;
using StudioCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Veldrid;
using static StudioCore.KeyBindings;
using Smithbox.Core.Core;

namespace Smithbox.Core.JSON;

// Common serializer context for JSON generation
[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

// Configuration Data
[JsonSerializable(typeof(CFG))]
[JsonSerializable(typeof(Bindings))]
[JsonSerializable(typeof(UI))]
[JsonSerializable(typeof(ProjectInstance))]
[JsonSerializable(typeof(ProjectDisplayConfig))]

[JsonSerializable(typeof(AliasStore))]
[JsonSerializable(typeof(AliasEntry))]

internal partial class SmithboxSerializerContext : JsonSerializerContext
{
}
