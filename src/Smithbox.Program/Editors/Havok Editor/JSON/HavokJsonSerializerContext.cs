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

internal partial class HavokJsonSerializerContext : JsonSerializerContext
{
}
