using StudioCore.Renderer;
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

[JsonSerializable(typeof(MaterialMeta))]
[JsonSerializable(typeof(MaterialClass))]
[JsonSerializable(typeof(MaterialField))]
[JsonSerializable(typeof(MaterialShaderTable))]
[JsonSerializable(typeof(List<MaterialDefinition>))]
[JsonSerializable(typeof(Dictionary<string, Dictionary<int, string>>))]
[JsonSerializable(typeof(Dictionary<string, Dictionary<int, MaterialTextureInput>>))]

internal partial class MaterialEditorJsonSerializerContext : JsonSerializerContext
{
}

