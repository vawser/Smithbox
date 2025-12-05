using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.Program.Editors.FmgEditor;

// Common serializer context for JSON generation
[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

[JsonSerializable(typeof(LanguageDef))]
[JsonSerializable(typeof(LanguageEntry))]
[JsonSerializable(typeof(ContainerDef))]
[JsonSerializable(typeof(FmgContainerEntry))]
[JsonSerializable(typeof(FmgFileEntry))]
[JsonSerializable(typeof(AssociationDef))]
[JsonSerializable(typeof(FmgAssociationGroup))]
[JsonSerializable(typeof(FmgAssociationEntry))]

internal partial class FmgJsonSerializerContext : JsonSerializerContext
{
}
