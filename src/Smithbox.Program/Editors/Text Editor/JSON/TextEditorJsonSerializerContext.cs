using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

[JsonSerializable(typeof(FmgDescriptors))]
[JsonSerializable(typeof(FmgDescriptor))]
[JsonSerializable(typeof(LanguageDescriptor))]
[JsonSerializable(typeof(FmgContainerDescriptor))]

[JsonSerializable(typeof(TextExportList))]
[JsonSerializable(typeof(TextExportEntry))]

internal partial class TextEditorJsonSerializerContext : JsonSerializerContext
{
}

