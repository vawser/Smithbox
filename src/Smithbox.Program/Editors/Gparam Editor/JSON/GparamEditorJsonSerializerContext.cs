using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

// Gparam Annotations
[JsonSerializable(typeof(GparamAnnotations))]
[JsonSerializable(typeof(GparamAnnotationEntry))]
[JsonSerializable(typeof(GparamAnnotationFieldEntry))]

[JsonSerializable(typeof(GparamAnnotationLanguages))]
[JsonSerializable(typeof(GparamAnnotationLanguageEntry))]

// Gparam Enums
[JsonSerializable(typeof(GparamEnums))]
[JsonSerializable(typeof(GparamEnumEntry))]
[JsonSerializable(typeof(GparamEnumOption))]
[JsonSerializable(typeof(GparamCategoryTextEntry))]

internal partial class GparamEditorJsonSerializerContext : JsonSerializerContext
{
}

