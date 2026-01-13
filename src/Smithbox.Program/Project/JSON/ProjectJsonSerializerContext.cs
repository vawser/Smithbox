using System.Text.Json.Serialization;

namespace StudioCore.Application;

// Common serializer context for JSON generation
[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

// Program
[JsonSerializable(typeof(CFG))]
[JsonSerializable(typeof(UI))]
[JsonSerializable(typeof(KeyBindings.Bindings))]
[JsonSerializable(typeof(KeyBind))]

// Project
[JsonSerializable(typeof(ProjectEntry))]
[JsonSerializable(typeof(LegacyProjectJSON))]

[JsonSerializable(typeof(FileDictionary))]
[JsonSerializable(typeof(FileDictionaryEntry))]

// Alias Data
[JsonSerializable(typeof(AliasStore))]
[JsonSerializable(typeof(AliasEntry))]

// Project Enums
[JsonSerializable(typeof(ProjectEnumResource))]
[JsonSerializable(typeof(ProjectEnumEntry))]
[JsonSerializable(typeof(ProjectEnumOption))]

// Format Information
[JsonSerializable(typeof(FormatResource))]
[JsonSerializable(typeof(FormatReference))]
[JsonSerializable(typeof(FormatMember))]

[JsonSerializable(typeof(FormatEnum))]
[JsonSerializable(typeof(FormatEnumEntry))]
[JsonSerializable(typeof(FormatEnumMember))]

[JsonSerializable(typeof(FormatMask))]
[JsonSerializable(typeof(FormatMaskEntry))]
[JsonSerializable(typeof(MaskSection))]


internal partial class ProjectJsonSerializerContext : JsonSerializerContext
{
}
