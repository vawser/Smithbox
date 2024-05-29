using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using StudioCore.Editors.MapEditor.MapGroup;

namespace StudioCore.Editors.MapEditor.SelectionGroup;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(SelectionGroupList))]
[JsonSerializable(typeof(SelectionGroupResource))]

public partial class SelectionGroupListSerializationContext
    : JsonSerializerContext
{ }

public class SelectionGroupList
{
    public List<SelectionGroupResource> Resources { get; set; }
}

public class SelectionGroupResource
{
    public string Name { get; set; }
    public int SelectionGroupKeybind { get; set; }
    public List<string> Tags { get; set; }
    public List<string> Selection { get; set; }
}
