using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Framework.MassEdit;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(MassEditTemplate))]
public partial class MassEditTemplateSerializationContext
    : JsonSerializerContext
{ }

public class MassEditTemplate
{
    public string Name { get; set; }
    public int MapLogic { get; set; }
    public int SelectionLogic { get; set; }
    public List<string> MapInputs { get; set; }
    public List<string> SelectionInputs { get; set; }
    public List<string> EditInputs { get; set; }
}