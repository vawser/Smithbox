using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

// Material Display Configuration
[JsonSerializable(typeof(MaterialDisplayConfiguration))]
[JsonSerializable(typeof(MaterialFileListConfiguration))]


internal partial class MaterialEditorJsonSerializerContext : JsonSerializerContext
{
}

