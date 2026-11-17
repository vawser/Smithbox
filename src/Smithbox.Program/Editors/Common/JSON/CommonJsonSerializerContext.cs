using System.Text.Json.Serialization;

namespace StudioCore.Application;

// Common serializer context for JSON generation
[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

[JsonSerializable(typeof(HavokMeta))]
[JsonSerializable(typeof(HavokClass))]
[JsonSerializable(typeof(HavokField))]

[JsonSerializable(typeof(StudioCore.Editors.Common.CharacterReloaderOffsets))]
[JsonSerializable(typeof(StudioCore.Editors.Common.CharacterReloaderOffsetEntry))]

internal partial class CommonJsonSerializerContext : JsonSerializerContext
{
}
