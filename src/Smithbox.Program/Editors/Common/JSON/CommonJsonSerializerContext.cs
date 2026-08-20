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

internal partial class CommonJsonSerializerContext : JsonSerializerContext
{
}
