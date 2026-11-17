using System.Text.Json.Serialization;

namespace StudioCore.Application;

// Common serializer context for JSON generation
[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

[JsonSerializable(typeof(HavokObjectAliases))]
[JsonSerializable(typeof(HavokAliasFileEntry))]
[JsonSerializable(typeof(HavokAliasEntry))]

[JsonSerializable(typeof(StudioCore.Editors.HavokEditor.HavokCharacterReloaderOffsets))]
[JsonSerializable(typeof(StudioCore.Editors.HavokEditor.HavokCharacterReloaderOffsetEntry))]

internal partial class HavokJsonSerializerContext : JsonSerializerContext
{
}
