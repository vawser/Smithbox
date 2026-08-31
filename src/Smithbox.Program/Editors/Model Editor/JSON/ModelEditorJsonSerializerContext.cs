using System.Text.Json.Serialization;

namespace StudioCore.Application;

// Common serializer context for JSON generation
[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

[JsonSerializable(typeof(ModelMeta))]
[JsonSerializable(typeof(ModelClass))]
[JsonSerializable(typeof(ModelField))]

[JsonSerializable(typeof(FlverDummyList))]
[JsonSerializable(typeof(FlverDummyRepresentation))]

[JsonSerializable(typeof(FlverNodeList))]
[JsonSerializable(typeof(FlverNodeRepresentation))]

internal partial class ModelEditorJsonSerializerContext : JsonSerializerContext
{
}
